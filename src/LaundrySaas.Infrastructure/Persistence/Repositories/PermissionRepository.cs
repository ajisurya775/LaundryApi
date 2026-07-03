using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LaundrySaas.Domain.Identity;

namespace LaundrySaas.Infrastructure.Persistence.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _db;

    public PermissionRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Permissions.ToListAsync(cancellationToken);
    }
}
