using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LaundrySaas.Domain.Identity;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Role>> GetRolesByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Role role, CancellationToken cancellationToken = default);
    void AddRange(IEnumerable<Role> roles);
}
