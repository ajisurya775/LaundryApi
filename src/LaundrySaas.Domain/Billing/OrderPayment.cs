using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Billing;

/// <summary>
/// Data detail pembayaran untuk Order.
/// </summary>
public class OrderPayment : Entity
{
    public Guid OrderId { get; private set; }
    
    // Hubungan wajib ke PaymentChannel
    public Guid PaymentChannelId { get; private set; }
    public PaymentChannel PaymentChannel { get; private set; } = null!;

    // Gateway integration fields
    public string? GatewayPaymentId { get; private set; }
    public string? GatewayReferenceId { get; private set; }
    public string? PaymentUrl { get; private set; }
    public string? PaymentCode { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? FailureReason { get; private set; }

    public OrderPayment(
        Guid id,
        Guid orderId,
        Guid paymentChannelId) : base(id)
    {
        OrderId = orderId;
        PaymentChannelId = paymentChannelId;
        GatewayReferenceId = $"PAY-{id:N}";
    }

    // EF Core Constructor
    private OrderPayment()
    {
        PaymentChannel = null!;
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
