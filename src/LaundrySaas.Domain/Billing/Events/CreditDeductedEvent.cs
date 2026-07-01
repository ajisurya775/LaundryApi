using LaundrySaas.SharedKernel.Interfaces;

namespace LaundrySaas.Domain.Billing.Events;

public record CreditDeductedEvent(
    Guid TenantId,
    decimal Amount,
    decimal NewBalance,
    OrderType TransactionType,
    string ReferenceNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
