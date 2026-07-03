using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LaundrySaas.Application.Contracts.Auth;
using LaundrySaas.Application.Identity.Services;

namespace LaundrySaas.Infrastructure.Persistence.Queries;

public class AuthQueryService : IAuthQueryService
{
    private readonly ApplicationDbContext _db;

    public AuthQueryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AuthResponse> BuildAuthResponseAsync(string token, DateTime expiry, Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == userId, cancellationToken);
        var tenant = await _db.Tenants.IgnoreQueryFilters().FirstAsync(t => t.Id == tenantId, cancellationToken);

        // 1. Load all employee assignments for this user
        var rawRoles = await _db.EmployeeAssignments
            .IgnoreQueryFilters()
            .Where(ua => ua.UserId == userId && ua.TenantId == tenantId)
            .Join(
                _db.Roles.IgnoreQueryFilters().Where(r => r.TenantId == tenantId && r.IsActive),
                ua => ua.RoleId,
                r => r.Id,
                (ua, r) => new { ua.BranchId, ua.PosId, RoleName = r.Name, RoleCode = r.Code })
            .ToListAsync(cancellationToken);

        // 2. Map branch details to the roles
        var branchIds = rawRoles.Where(r => r.BranchId.HasValue).Select(r => r.BranchId!.Value).Distinct().ToList();
        var branchesMap = await _db.Branches
            .IgnoreQueryFilters()
            .Where(b => branchIds.Contains(b.Id) && b.TenantId == tenantId && b.IsActive)
            .ToDictionaryAsync(b => b.Id, b => b.Name, cancellationToken);

        var rolesList = rawRoles.Select(r => new UserBranchDto(
            r.BranchId,
            r.BranchId.HasValue && branchesMap.TryGetValue(r.BranchId.Value, out var name) ? name : null,
            r.RoleName,
            r.RoleCode
        )).ToList();

        // 3. Load permissions
        var permissionKeys = await _db.EmployeeAssignments
            .IgnoreQueryFilters()
            .Where(ua => ua.UserId == userId && ua.TenantId == tenantId)
            .Join(
                _db.RolePermissions.IgnoreQueryFilters().Where(rp => rp.TenantId == tenantId),
                ua => ua.RoleId,
                rp => rp.RoleId,
                (ua, rp) => rp.PermissionId)
            .Join(
                _db.Permissions.Where(p => p.IsActive),
                permId => permId,
                p => p.Id,
                (permId, p) => p.Key)
            .Distinct()
            .ToListAsync(cancellationToken);

        // 4. Load branches that user has explicit access to (either explicitly assigned or via tenant-wide assignment)
        var hasTenantWideRole = rawRoles.Any(r => r.BranchId == null);
        List<BranchDto> branches;

        if (hasTenantWideRole)
        {
            branches = await _db.Branches
                .IgnoreQueryFilters()
                .Where(b => b.TenantId == tenantId && b.IsActive)
                .Select(b => new BranchDto(b.Id, b.Name, b.Address))
                .ToListAsync(cancellationToken);
        }
        else
        {
            branches = await _db.Branches
                .IgnoreQueryFilters()
                .Where(b => branchIds.Contains(b.Id) && b.TenantId == tenantId && b.IsActive)
                .Select(b => new BranchDto(b.Id, b.Name, b.Address))
                .ToListAsync(cancellationToken);
        }

        var resolvedBranchIds = branches.Select(b => b.Id).ToList();

        // 5. Load active POS assignments from EmployeeAssignments
        var now = DateTime.UtcNow;
        var poses = await _db.EmployeeAssignments
            .IgnoreQueryFilters()
            .Where(ua => ua.UserId == userId && ua.TenantId == tenantId && ua.IsActive && ua.EffectiveFrom <= now && (ua.EffectiveUntil == null || ua.EffectiveUntil >= now) && ua.PosId != null)
            .Join(
                _db.Poses.IgnoreQueryFilters().Where(p => p.TenantId == tenantId && p.IsActive && resolvedBranchIds.Contains(p.BranchId)),
                ua => ua.PosId,
                p => p.Id,
                (ua, p) => new { ua.IsDefault, Pos = p })
            .Join(
                _db.Branches.IgnoreQueryFilters().Where(b => b.TenantId == tenantId),
                x => x.Pos.BranchId,
                b => b.Id,
                (x, b) => new { x.IsDefault, PosDto = new PosDto(x.Pos.Id, x.Pos.Name, b.Id, b.Name) })
            .ToListAsync(cancellationToken);

        var posDtos = poses.Select(p => p.PosDto).ToList();
        var defaultPos = poses.FirstOrDefault(p => p.IsDefault)?.PosDto ?? posDtos.FirstOrDefault();

        return new AuthResponse(
            token,
            expiry,
            new UserDto(userId, user.Email, user.FullName, tenantId, tenant.Name),
            rolesList,
            permissionKeys,
            branches,
            posDtos,
            defaultPos
        );
    }
}
