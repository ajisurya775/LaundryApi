using System;

namespace LaundrySaas.Application.Billing.Services;

public record GatewayPaymentResult(
    bool IsSuccess,
    string? GatewayPaymentId,
    string? PaymentUrl,
    string? PaymentCode,
    DateTime? ExpiresAt,
    string? ErrorMessage);

public record GatewayPaymentStatus(
    string GatewayPaymentId,
    string Status, // "Pending", "Succeeded", "Failed", "Expired"
    DateTime? PaidAt,
    string? FailureReason);
