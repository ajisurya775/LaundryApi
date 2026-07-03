using System.Threading;
using System.Threading.Tasks;

namespace LaundrySaas.SharedKernel.Interfaces;

public interface ICommandHandler<in TCommand, TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
