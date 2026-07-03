using System.Collections.Generic;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Billing;

/// <summary>
/// Owned entity that holds the price breakdown of an Invoice.
/// Recalculated automatically when items, discount, or tax change.
/// </summary>
public class InvoicePriceDetails : ValueObject
{
    public Money Subtotal { get; private set; }
    public Money DiscountAmount { get; private set; }
    public string? TaxName { get; private set; }
    public TaxRate? TaxRate { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money GrandTotal { get; private set; }
    public Money TotalPaid { get; private set; }
    public Money TotalDue { get; private set; }

    internal InvoicePriceDetails(string currencyCode)
    {
        Subtotal = Money.Zero(currencyCode);
        DiscountAmount = Money.Zero(currencyCode);
        TaxAmount = Money.Zero(currencyCode);
        GrandTotal = Money.Zero(currencyCode);
        TotalPaid = Money.Zero(currencyCode);
        TotalDue = Money.Zero(currencyCode);
    }

    private InvoicePriceDetails()
    {
        Subtotal = null!;
        DiscountAmount = null!;
        TaxAmount = null!;
        GrandTotal = null!;
        TotalPaid = null!;
        TotalDue = null!;
    }

    internal void Recalculate(Money subtotal, Money discountAmount, string? taxName, TaxRate? taxRate, Money totalPaid)
    {
        Subtotal = subtotal;
        DiscountAmount = discountAmount;
        TaxName = taxName;
        TaxRate = taxRate;

        var afterDiscount = subtotal.Subtract(discountAmount);

        if (taxRate != null && taxRate.Rate > 0)
        {
            TaxAmount = afterDiscount.Multiply(taxRate.Rate / 100m);
        }
        else
        {
            TaxAmount = Money.Zero(subtotal.CurrencyCode);
        }

        GrandTotal = afterDiscount.Add(TaxAmount);
        TotalPaid = totalPaid;
        TotalDue = GrandTotal.Subtract(totalPaid);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Subtotal;
        yield return DiscountAmount;
        yield return TaxName ?? string.Empty;
        yield return TaxRate?.Rate ?? 0m;
        yield return TaxAmount;
        yield return GrandTotal;
        yield return TotalPaid;
        yield return TotalDue;
    }
}
