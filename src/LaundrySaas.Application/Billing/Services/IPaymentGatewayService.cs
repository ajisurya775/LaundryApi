using System;
using System.Threading;
using System.Threading.Tasks;

namespace LaundrySaas.Application.Billing.Services;

public interface IPaymentGatewayService
{
    Guid GatewayId { get; }

    Task<GatewayPaymentResult> CreatePaymentAsync(
        decimal amount,
        string channelCode,
        string referenceId,
        string? customerEmail,
        string? description,
        CancellationToken cancellationToken = default);

    Task<GatewayPaymentStatus> GetPaymentStatusAsync(
        string gatewayPaymentId,
        CancellationToken cancellationToken = default);
}
