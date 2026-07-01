using Microsoft.AspNetCore.Http;

namespace LaundrySaas.Api.Middleware;

public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;
    private const string TenantHeaderName = "X-Tenant-Id";

    public TenantResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip tenant resolution for non-API endpoints (e.g. Swagger, OpenAPI)
        if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) && 
            !path.Equals("/weatherforecast", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (context.Request.Headers.TryGetValue(TenantHeaderName, out var tenantHeaderValues))
        {
            var tenantHeader = tenantHeaderValues.FirstOrDefault();
            if (Guid.TryParse(tenantHeader, out var tenantId))
            {
                context.Items["TenantId"] = tenantId;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid X-Tenant-Id format. It must be a valid GUID.");
                return;
            }
        }
        else
        {
            // Allow bypassing tenant check for public API endpoints (e.g., registration) or weatherforecast
            if (path.Equals("/api/v1/tenants/register", StringComparison.OrdinalIgnoreCase) || 
                path.Equals("/weatherforecast", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("X-Tenant-Id header is missing.");
            return;
        }

        await _next(context);
    }
}
