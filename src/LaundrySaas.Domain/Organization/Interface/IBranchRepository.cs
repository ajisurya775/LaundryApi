using System;
using System.Threading;
using System.Threading.Tasks;

namespace LaundrySaas.Domain.Organization;

public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Branch branch, CancellationToken cancellationToken = default);
}
