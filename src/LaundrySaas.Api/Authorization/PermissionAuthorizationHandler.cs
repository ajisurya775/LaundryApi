using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.Infrastructure.Persistence;

namespace LaundrySaas.Api.Authorization;

/// <summary>
/// Authorization handler that checks if the current user has the required permission.
/// Scoped to the current active branch (via X-Branch-Id) and tenant-wide permissions.
/// Permissions are cached per-user-tenant-branch in IMemoryCache to protect DB performance.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _memoryCache;
    private readonly IBranchProvider _branchProvider;

    public PermissionAuthorizationHandler(
        IServiceScopeFactory scopeFactory,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache memoryCache,
        IBranchProvider branchProvider)
    {
        _scopeFactory = scopeFactory;
        _httpContextAccessor = httpContextAccessor;
        _memoryCache = memoryCache;
        _branchProvider = branchProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirst("sub");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return;
        }

        var tenantIdClaim = context.User.FindFirst("tenantId");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            return;
        }

        // Get active branch ID if present
        var branchId = _branchProvider.BranchId;

        var permissions = await GetUserPermissionsCachedAsync(userId, tenantId, branchId);

        if (permissions.Contains(requirement.PermissionKey, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }
    }

    private async Task<HashSet<string>> GetUserPermissionsCachedAsync(Guid userId, Guid tenantId, Guid? branchId)
    {
        var cacheKey = $"perms:{userId}:{tenantId}:{branchId?.ToString() ?? "global"}";

        if (_memoryCache.TryGetValue(cacheKey, out HashSet<string>? cachedPermissions) && cachedPermissions != null)
        {
            return cachedPermissions;
        }

        // Fresh database fetch via new scope to avoid DbContext thread safety issues
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Load permissions where BranchId is either null (tenant-wide) OR matches the active branch ID
        var permissions = await db.EmployeeAssignments
            .IgnoreQueryFilters()
            .Where(ua => ua.UserId == userId && ua.TenantId == tenantId && (ua.BranchId == null || ua.BranchId == branchId))
            .Join(
                db.RolePermissions.IgnoreQueryFilters().Where(rp => rp.TenantId == tenantId),
                ua => ua.RoleId,
                rp => rp.RoleId,
                (ua, rp) => rp.PermissionId)
            .Join(
                db.Permissions.Where(p => p.IsActive),
                permId => permId,
                p => p.Id,
                (permId, p) => p.Key)
            .Distinct()
            .ToListAsync();

        var permissionSet = new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);

        // Cache for 10 minutes sliding expiration
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10));

        _memoryCache.Set(cacheKey, permissionSet, cacheEntryOptions);

        return permissionSet;
    }
}
