using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LaundrySaas.Domain.Billing;

public interface ISubscriptionPlanRepository
{
    Task<List<SubscriptionPlan>> GetAllAsync(CancellationToken cancellationToken = default);
}
