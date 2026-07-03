using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LaundrySaas.Domain.Organization;

namespace LaundrySaas.Infrastructure.Persistence.Repositories;

public class BranchRepository : IBranchRepository
{
    private readonly ApplicationDbContext _db;

    public BranchRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Branches.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task AddAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        await _db.Branches.AddAsync(branch, cancellationToken);
    }
}
