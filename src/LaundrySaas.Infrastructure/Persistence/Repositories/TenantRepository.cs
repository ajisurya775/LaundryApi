using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LaundrySaas.Domain.Organization;

namespace LaundrySaas.Infrastructure.Persistence.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _db;

    public TenantRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _db.Tenants.AnyAsync(t => t.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _db.Tenants.AddAsync(tenant, cancellationToken);
    }

    public void Update(Tenant tenant)
    {
        _db.Tenants.Update(tenant);
    }
}
