using System;
using System.Collections.Generic;
using System.Linq;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Billing;

public class Invoice : AggregateRoot, IMustHaveTenant
{
    private readonly List<InvoiceItem> _items = new();
    private readonly List<InvoicePayment> _payments = new();

    public Guid TenantId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public string Number { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public string CurrencyCode { get; private set; }
    public Money Discount { get; private set; }
    public Money Subtotal { get; private set; }
    public Money GrandTotal { get; private set; }
    public string? Notes { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime? DueAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<InvoicePayment> Payments => _payments.AsReadOnly();
    public InvoicePriceDetails PriceDetails { get; private set; }

    // --- Discount & Tax backing fields ---
    private Money _discountAmount;
    private string? _taxName;
    private TaxRate? _taxRate;

    public Invoice(
        Guid id,
        Guid tenantId,
        string number,
        DateTime issuedAt,
        string currencyCode = "IDR",
        Guid? customerId = null,
        DateTime? dueAt = null,
        string? notes = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Invoice number is required.", nameof(number));

        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code is required.", nameof(currencyCode));

        TenantId = tenantId;
        CustomerId = customerId;
        Number = number;
        CurrencyCode = currencyCode.ToUpperInvariant();
        IssuedAt = issuedAt;
        DueAt = dueAt;
        Notes = notes;
        Status = InvoiceStatus.Draft;
        Discount = Money.Zero(CurrencyCode);
        Subtotal = Money.Zero(CurrencyCode);
        GrandTotal = Money.Zero(CurrencyCode);

        _discountAmount = Money.Zero(CurrencyCode);
        PriceDetails = new InvoicePriceDetails(CurrencyCode);
    }

    private Invoice()
    {
        Number = null!;
        CurrencyCode = null!;
        Discount = null!;
        Subtotal = null!;
        GrandTotal = null!;
        PriceDetails = null!;
        _discountAmount = null!;
    }

    // ============================
    // Item Management
    // ============================

    public void AddItem(string name, Money unitPrice, int quantity, string? description = null)
    {
        EnsureModifiable("add item");
        EnsureSameCurrency(unitPrice, "item price");

        var item = new InvoiceItem(Guid.NewGuid(), Id, name, unitPrice, quantity, description);
        _items.Add(item);
        RecalculatePriceDetails();
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureModifiable("remove item");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException($"Item with ID '{itemId}' not found on this invoice.");

        _items.Remove(item);
        RecalculatePriceDetails();
    }

    // ============================
    // Discount & Tax
    // ============================

    public void SetDiscount(Money amount)
    {
        EnsureModifiable("set discount");
        EnsureSameCurrency(amount, "discount");

        if (amount.IsNegative)
            throw new ArgumentException("Discount amount cannot be negative.", nameof(amount));

        _discountAmount = amount;
        RecalculatePriceDetails();
    }

    public void SetTax(string name, TaxRate rate)
    {
        EnsureModifiable("set tax");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tax name is required.", nameof(name));

        _taxName = name;
        _taxRate = rate ?? throw new ArgumentNullException(nameof(rate));
        RecalculatePriceDetails();
    }

    public void ClearTax()
    {
        EnsureModifiable("clear tax");

        _taxName = null;
        _taxRate = null;
        RecalculatePriceDetails();
    }

    // ============================
    // Payment Management
    // ============================

    public void AddPayment(Guid paymentMethodId, Money amount, DateTime paidAt, string? referenceNumber = null, string? notes = null)
    {
        if (Status == InvoiceStatus.Void)
            throw new InvalidOperationException("Cannot add payment to a voided invoice.");

        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Invoice is already fully paid.");

        if (Status == InvoiceStatus.Draft)
            throw new InvalidOperationException("Cannot add payment to a draft invoice. Issue it first.");

        EnsureSameCurrency(amount, "payment");

        var payment = new InvoicePayment(Guid.NewGuid(), Id, paymentMethodId, amount, paidAt, referenceNumber, notes);
        _payments.Add(payment);

        RecalculatePriceDetails();
        UpdatePaymentStatus();
    }

    // ============================
    // Status Transitions
    // ============================

    public void Issue()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException($"Cannot issue invoice — current status is '{Status}', expected 'Draft'.");

        if (!_items.Any())
            throw new InvalidOperationException("Cannot issue an invoice with no items.");

        Status = InvoiceStatus.Unpaid;
    }

    public void MarkAsVoid()
    {
        if (Status == InvoiceStatus.Void)
            throw new InvalidOperationException("Invoice is already voided.");

        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Cannot void a paid invoice.");

        Status = InvoiceStatus.Void;
    }

    public void MarkAsOverdue()
    {
        if (Status != InvoiceStatus.Unpaid && Status != InvoiceStatus.PartialPaid)
            throw new InvalidOperationException($"Cannot mark as overdue — current status is '{Status}'.");

        Status = InvoiceStatus.Overdue;
    }

    // ============================
    // Internal Helpers
    // ============================

    private void RecalculatePriceDetails()
    {
        var subtotal = Money.Zero(CurrencyCode);
        foreach (var item in _items)
        {
            subtotal = subtotal.Add(item.TotalPrice);
        }

        var totalPaid = Money.Zero(CurrencyCode);
        foreach (var payment in _payments)
        {
            totalPaid = totalPaid.Add(payment.Amount);
        }

        PriceDetails.Recalculate(subtotal, _discountAmount, _taxName, _taxRate, totalPaid);

        // Sync top-level properties
        Subtotal = PriceDetails.Subtotal;
        Discount = PriceDetails.DiscountAmount;
        GrandTotal = PriceDetails.GrandTotal;
    }

    private void UpdatePaymentStatus()
    {
        if (PriceDetails.TotalDue.IsZero || PriceDetails.TotalDue.IsNegative)
        {
            Status = InvoiceStatus.Paid;
            PaidAt = DateTime.UtcNow;
        }
        else if (PriceDetails.TotalPaid.Amount > 0)
        {
            Status = InvoiceStatus.PartialPaid;
        }
    }

    private void EnsureModifiable(string action)
    {
        if (Status == InvoiceStatus.Paid || Status == InvoiceStatus.Void)
            throw new InvalidOperationException($"Cannot {action} — invoice is in status '{Status}'.");
    }

    private void EnsureSameCurrency(Money money, string context)
    {
        if (money.CurrencyCode != CurrencyCode)
            throw new InvalidOperationException($"Currency mismatch on {context}: expected '{CurrencyCode}', got '{money.CurrencyCode}'.");
    }
}
