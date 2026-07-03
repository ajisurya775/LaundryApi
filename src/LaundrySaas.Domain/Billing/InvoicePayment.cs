using System;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Billing;

public class InvoicePayment : Entity
{
    public Guid InvoiceId { get; private set; }
    public Guid PaymentMethodId { get; private set; }
    public Money Amount { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Notes { get; private set; }
    public string? PaymentLink { get; private set; }
    public DateTime PaidAt { get; private set; }

    public InvoicePayment(
        Guid id,
        Guid invoiceId,
        Guid paymentMethodId,
        Money amount,
        DateTime paidAt,
        string? referenceNumber = null,
        string? notes = null,
        string? paymentLink = null
        ) : base(id)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));

        if (amount.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

        InvoiceId = invoiceId;
        PaymentMethodId = paymentMethodId;
        Amount = amount;
        PaidAt = paidAt;
        ReferenceNumber = referenceNumber;
        Notes = notes;
        PaymentLink = paymentLink;
    }

    private InvoicePayment()
    {
        Amount = null!;
    }
}
