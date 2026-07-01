using LaundrySaas.Domain.Billing.Events;
using LaundrySaas.SharedKernel.Interfaces;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Billing;

/// <summary>
/// Aggregate root or major Entity representing a Billing Order (Purchase/Deduction/Refund/Adjustment).
/// </summary>
public class Order : Entity, IMustHaveTenant
{
    private readonly List<OrderItem> _items = new();

    public Guid TenantId { get; private set; }
    public Guid CreditBalanceId { get; private set; }
    public OrderType Type { get; private set; }
    public OrderStatus Status { get; private set; }
    public string CurrencyCode { get; private set; }
    public Money SubTotal { get; private set; }
    public Money FeeAmount { get; private set; }
    public Money GrandTotal { get; private set; }
    public string ReferenceNumber { get; private set; }
    public string Description { get; private set; }
    public Guid? PerformedBy { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public OrderPayment? Payment { get; private set; }

    public Order(
        Guid id,
        Guid tenantId,
        Guid creditBalanceId,
        OrderType type,
        string referenceNumber,
        string description,
        string currencyCode = "IDR",
        Guid? performedBy = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(referenceNumber))
            throw new ArgumentException("Reference number is required.", nameof(referenceNumber));

        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code is required.", nameof(currencyCode));

        TenantId = tenantId;
        CreditBalanceId = creditBalanceId;
        Type = type;
        ReferenceNumber = referenceNumber;
        Description = description;
        PerformedBy = performedBy;
        Status = OrderStatus.Pending;
        CurrencyCode = currencyCode.ToUpperInvariant();
        SubTotal = Money.Zero(CurrencyCode);
        FeeAmount = Money.Zero(CurrencyCode);
        GrandTotal = Money.Zero(CurrencyCode);
    }

    // EF Core Constructor
    private Order()
    {
        ReferenceNumber = null!;
        Description = null!;
        CurrencyCode = null!;
        SubTotal = null!;
        FeeAmount = null!;
        GrandTotal = null!;
    }

    public void AddItem(string name, Money unitPrice, int quantity, decimal creditAmount = 0, Guid? creditPackageId = null)
    {
        EnsureNotTerminal("add item");

        if (unitPrice == null)
            throw new ArgumentNullException(nameof(unitPrice));

        if (unitPrice.CurrencyCode != CurrencyCode)
            throw new InvalidOperationException($"Cannot add item with currency '{unitPrice.CurrencyCode}' to an order in '{CurrencyCode}'.");

        var item = new OrderItem(Guid.NewGuid(), Id, name, unitPrice, quantity, creditAmount, creditPackageId);
        _items.Add(item);
        
        SubTotal = SubTotal.Add(item.TotalPrice);
        RecalculateGrandTotal();
    }

    public void SetupGatewayPayment(PaymentChannel paymentChannel)
    {
        if (paymentChannel == null)
            throw new ArgumentNullException(nameof(paymentChannel));

        EnsureStatus(OrderStatus.Pending, "setup payment");
        Payment = new OrderPayment(Guid.NewGuid(), Id, paymentChannel.Id);
        
        // Calculate fee based on current subtotal using the channel's fee configuration
        FeeAmount = paymentChannel.CalculateFee(SubTotal);
        RecalculateGrandTotal();
    }

    public void AssignGatewayDetails(string gatewayPaymentId, string? paymentUrl, string? paymentCode, DateTime? expiresAt)
    {
        if (Payment == null)
            throw new InvalidOperationException("No payment details set up for this order.");

        Payment.AssignGatewayReference(gatewayPaymentId, paymentUrl, paymentCode, expiresAt);
    }

    public void MarkAsProcessing()
    {
        EnsureStatus(OrderStatus.Pending, "mark as processing");
        Status = OrderStatus.Processing;
    }

    public void MarkAsSucceeded(DateTime paidAt)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Processing)
            throw new InvalidOperationException($"Cannot succeed order from status {Status}.");

        Status = OrderStatus.Succeeded;
        Payment?.MarkAsPaid(paidAt);
    }

    public void MarkAsFailed(string reason)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Processing)
            throw new InvalidOperationException($"Cannot fail order from status {Status}.");

        Status = OrderStatus.Failed;
        Payment?.MarkAsFailed(reason);
    }

    public void MarkAsExpired()
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Processing)
            throw new InvalidOperationException($"Cannot expire order from status {Status}.");

        Status = OrderStatus.Expired;
        Payment?.MarkAsExpired();
    }

    public void Cancel()
    {
        EnsureStatus(OrderStatus.Pending, "cancel");
        Status = OrderStatus.Cancelled;
    }

    /// <summary>
    /// Total kredit yang didapat atau dipotong dari seluruh order item di order ini.
    /// </summary>
    public decimal GetTotalCreditAmount()
    {
        decimal total = 0;
        foreach (var item in _items)
        {
            total += item.CreditAmount;
        }
        return total;
    }

    public bool IsTerminal => Status is OrderStatus.Succeeded
        or OrderStatus.Failed
        or OrderStatus.Expired
        or OrderStatus.Cancelled;

    private void RecalculateGrandTotal()
    {
        GrandTotal = SubTotal.Add(FeeAmount);
    }

    private void EnsureStatus(OrderStatus expected, string action)
    {
        if (Status != expected)
            throw new InvalidOperationException($"Cannot {action} — current status is '{Status}', expected '{expected}'.");
    }

    private void EnsureNotTerminal(string action)
    {
        if (IsTerminal)
            throw new InvalidOperationException($"Cannot {action} — order is in terminal status '{Status}'.");
    }
}
