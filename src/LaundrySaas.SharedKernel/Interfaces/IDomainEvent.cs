namespace LaundrySaas.SharedKernel.Interfaces;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
