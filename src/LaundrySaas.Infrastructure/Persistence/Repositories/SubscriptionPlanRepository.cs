using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LaundrySaas.Domain.Billing;

namespace LaundrySaas.Infrastructure.Persistence.Repositories;

public class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    private readonly ApplicationDbContext _db;

    public SubscriptionPlanRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<SubscriptionPlan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SubscriptionPlans
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
