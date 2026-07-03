using System;
using System.Threading;
using System.Threading.Tasks;

namespace LaundrySaas.Domain.POS;

public interface IPosRepository
{
    Task<Pos?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Pos pos, CancellationToken cancellationToken = default);
}
