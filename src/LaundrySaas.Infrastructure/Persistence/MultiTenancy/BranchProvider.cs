using System;
using Microsoft.AspNetCore.Http;
using LaundrySaas.SharedKernel.MultiTenancy;

namespace LaundrySaas.Infrastructure.Persistence.MultiTenancy;

public class BranchProvider : IBranchProvider
{
    private const string BranchHeaderName = "X-Branch-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BranchProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? BranchId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return null;
            }

            if (httpContext.Request.Headers.TryGetValue(BranchHeaderName, out var branchHeader) &&
                Guid.TryParse(branchHeader, out var branchId))
            {
                return branchId;
            }

            return null;
        }
    }
}
