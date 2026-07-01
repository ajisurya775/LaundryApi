using LaundrySaas.SharedKernel.Interfaces;

namespace LaundrySaas.Domain.Billing.Events;

public record CreditToppedUpEvent(
    Guid TenantId,
    decimal Amount,
    decimal NewBalance,
    string ReferenceNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
