using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using LaundrySaas.Application.Common;

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
                await WriteErrorAsync(context, "Invalid X-Tenant-Id format. It must be a valid GUID.");
                return;
            }
        }
        else
        {
            // Allow bypassing tenant check for public API endpoints (e.g., registration, login, complete-profile) or weatherforecast
            if (path.Equals("/api/v1/tenants/register", StringComparison.OrdinalIgnoreCase) || 
                path.Equals("/api/v1/tenants/complete-profile", StringComparison.OrdinalIgnoreCase) || 
                path.Equals("/api/v1/tenants/login", StringComparison.OrdinalIgnoreCase) || 
                path.Equals("/api/v1/tenants/login/firebase", StringComparison.OrdinalIgnoreCase) || 
                path.Equals("/api/v1/plans", StringComparison.OrdinalIgnoreCase) || 
                path.Equals("/weatherforecast", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            await WriteErrorAsync(context, "X-Tenant-Id header is missing.");
            return;
        }

        await _next(context);
    }
    private static async Task WriteErrorAsync(HttpContext context, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        
        var apiResponse = ApiResponse.CreateError(message, message);
        var json = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
        
        await context.Response.WriteAsync(json);
    }
}
