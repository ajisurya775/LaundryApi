using LaundrySaas.Domain.Billing;

namespace LaundrySaas.Application.Abstractions;

/// <summary>
/// Provider-agnostic interface for payment gateway operations.
/// Implement this for each payment provider (Xendit, Midtrans, Doku, etc.)
/// in the Infrastructure layer.
///
/// Example implementations:
/// - XenditPaymentGatewayService : IPaymentGatewayService
/// - MidtransPaymentGatewayService : IPaymentGatewayService
/// </summary>
public interface IPaymentGatewayService
{
    /// <summary>
    /// The provider entity ID this implementation handles.
    /// </summary>
    Guid ProviderId { get; }

    /// <summary>
    /// Creates a payment request with the gateway.
    /// Returns a result containing the gateway's payment ID, payment URL/code, and expiry.
    /// </summary>
    Task<GatewayPaymentResult> CreatePaymentAsync(
        decimal amount,
        PaymentChannel channel,
        string referenceId,
        string? customerEmail,
        string? description,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the gateway for the current status of a payment.
    /// Used as a fallback when webhooks are delayed or missed.
    /// </summary>
    Task<GatewayPaymentStatus> GetPaymentStatusAsync(
        string gatewayPaymentId,
        CancellationToken cancellationToken = default);
}
