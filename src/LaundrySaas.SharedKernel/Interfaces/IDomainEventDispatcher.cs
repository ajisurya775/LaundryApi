using System.Threading;
using System.Threading.Tasks;

namespace LaundrySaas.SharedKernel.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
