using LaundrySaas.Domain.Billing;

namespace LaundrySaas.Application.Abstractions;

/// <summary>
/// Result returned by a payment gateway after creating a payment request.
/// Provider-agnostic — all gateway implementations return this same record.
/// </summary>
public record GatewayPaymentResult(
    bool IsSuccess,
    string? GatewayPaymentId,
    string? PaymentUrl,
    string? PaymentCode,
    DateTime? ExpiresAt,
    string? ErrorMessage);

/// <summary>
/// Status of a payment as reported by the payment gateway.
/// </summary>
public record GatewayPaymentStatus(
    string GatewayPaymentId,
    OrderStatus Status,
    DateTime? PaidAt,
    string? FailureReason);
