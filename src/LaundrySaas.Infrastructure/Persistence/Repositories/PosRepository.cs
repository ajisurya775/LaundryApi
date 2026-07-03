using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LaundrySaas.Domain.POS;

namespace LaundrySaas.Infrastructure.Persistence.Repositories;

public class PosRepository : IPosRepository
{
    private readonly ApplicationDbContext _db;

    public PosRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Pos?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Poses.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(Pos pos, CancellationToken cancellationToken = default)
    {
        await _db.Poses.AddAsync(pos, cancellationToken);
    }
}
