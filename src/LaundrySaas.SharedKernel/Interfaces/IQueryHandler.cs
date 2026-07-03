using System.Threading;
using System.Threading.Tasks;

namespace LaundrySaas.SharedKernel.Interfaces;

public interface IQueryHandler<in TQuery, TResponse>
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
