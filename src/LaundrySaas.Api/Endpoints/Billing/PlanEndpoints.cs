using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using LaundrySaas.Application.Common;
using LaundrySaas.Application.Billing.GetPlans;
using LaundrySaas.Application.Contracts.Billing;
using LaundrySaas.SharedKernel.Interfaces;

namespace LaundrySaas.Api.Endpoints.Billing;

public static class PlanEndpoints
{
    public static void MapPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/plans");

        group.MapGet("/", GetPlansAsync)
            .WithName("GetPlans")
            .WithOpenApi();
    }

    private static async Task<IResult> GetPlansAsync(
        IQueryHandler<GetSubscriptionPlansQuery, ApiResponse<List<SubscriptionPlanDto>>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetSubscriptionPlansQuery();
        var result = await handler.HandleAsync(query, cancellationToken);
        
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }
}
