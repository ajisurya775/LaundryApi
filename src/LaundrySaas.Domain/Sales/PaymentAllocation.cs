using System;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Sales;

public class PaymentAllocation : Entity
{
    public Guid OrderId { get; private set; }
    public Guid PaymentMethodId { get; private set; }

    // Gateway integration fields
    public string? GatewayPaymentId { get; private set; }
    public string? GatewayReferenceId { get; private set; }
    public string? PaymentUrl { get; private set; }
    public string? PaymentCode { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? FailureReason { get; private set; }

    public PaymentAllocation(
        Guid id,
        Guid orderId,
        Guid paymentMethodId) : base(id)
    {
        OrderId = orderId;
        PaymentMethodId = paymentMethodId;
        GatewayReferenceId = $"PAY-{id:N}";
    }

    private PaymentAllocation()
    {
    }

    internal void AssignGatewayReference(
        string gatewayPaymentId,
        string? paymentUrl,
        string? paymentCode,
        DateTime? expiresAt)
    {
        if (string.IsNullOrWhiteSpace(gatewayPaymentId))
            throw new ArgumentException("Gateway payment ID is required.", nameof(gatewayPaymentId));

        GatewayPaymentId = gatewayPaymentId;
        PaymentUrl = paymentUrl;
        PaymentCode = paymentCode;
        ExpiresAt = expiresAt;
    }

    internal void MarkAsPaid(DateTime paidAt)
    {
        PaidAt = paidAt;
    }

    internal void MarkAsFailed(string reason)
    {
        FailureReason = reason;
    }

    internal void MarkAsExpired()
    {
        FailureReason = "Payment expired.";
    }
}
