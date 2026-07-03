using System;
using System.Threading;
using System.Threading.Tasks;

namespace LaundrySaas.Domain.Organization;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    void Update(Tenant tenant);
}
