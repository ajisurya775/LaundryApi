using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LaundrySaas.Application.Common;
using LaundrySaas.Application.Contracts.Billing;
using LaundrySaas.Domain.Billing;
using LaundrySaas.SharedKernel.Interfaces;

namespace LaundrySaas.Application.Billing.GetPlans;

public class GetSubscriptionPlansQueryHandler : IQueryHandler<GetSubscriptionPlansQuery, ApiResponse<List<SubscriptionPlanDto>>>
{
    private readonly ISubscriptionPlanRepository _repository;

    public GetSubscriptionPlansQueryHandler(ISubscriptionPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<SubscriptionPlanDto>>> HandleAsync(GetSubscriptionPlansQuery query, CancellationToken cancellationToken = default)
    {
        var plans = await _repository.GetAllAsync(cancellationToken);

        var planDtos = plans.Select(p => new SubscriptionPlanDto(
            p.Id,
            p.Name,
            p.Price.Amount,
            p.Price.Currency.Code,
            p.HasPos,
            p.HasInventory,
            p.HasAccounting,
            p.ExtraCredit,
            p.IsActive
        )).ToList();

        return ApiResponse<List<SubscriptionPlanDto>>.CreateSuccess(planDtos, "Plans retrieved successfully.");
    }
}
