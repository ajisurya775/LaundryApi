using System;
using Microsoft.AspNetCore.Http;
using LaundrySaas.SharedKernel.MultiTenancy;

namespace LaundrySaas.Infrastructure.Persistence.MultiTenancy;

public class TenantProvider : ITenantProvider
{
    private const string TenantHeaderName = "X-Tenant-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return null;
            }

            if (httpContext.Items.TryGetValue("TenantId", out var tenantIdObj) && 
                tenantIdObj is Guid tenantIdGuid)
            {
                return tenantIdGuid;
            }

            if (httpContext.Request.Headers.TryGetValue(TenantHeaderName, out var tenantHeader) &&
                Guid.TryParse(tenantHeader, out var tenantIdFromHeader))
            {
                return tenantIdFromHeader;
            }

            return null;
        }
    }
}
