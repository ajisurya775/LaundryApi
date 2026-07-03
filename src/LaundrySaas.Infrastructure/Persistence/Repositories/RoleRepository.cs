using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LaundrySaas.Domain.Identity;

namespace LaundrySaas.Infrastructure.Persistence.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _db;

    public RoleRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<Role>> GetRolesByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _db.Roles.IgnoreQueryFilters().Where(r => r.TenantId == tenantId).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _db.Roles.AddAsync(role, cancellationToken);
    }

    public void AddRange(IEnumerable<Role> roles)
    {
        _db.Roles.AddRange(roles);
    }
}
