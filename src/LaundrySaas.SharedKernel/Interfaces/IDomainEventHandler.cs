using System.Threading;
using System.Threading.Tasks;

namespace LaundrySaas.SharedKernel.Interfaces;

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
