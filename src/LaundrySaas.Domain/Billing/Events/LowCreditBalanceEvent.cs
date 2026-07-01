using LaundrySaas.SharedKernel.Interfaces;

namespace LaundrySaas.Domain.Billing.Events;

public record LowCreditBalanceEvent(
    Guid TenantId,
    decimal CurrentBalance,
    decimal Threshold) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
