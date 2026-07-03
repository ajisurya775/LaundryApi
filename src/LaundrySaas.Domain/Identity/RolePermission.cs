using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Identity;

public class RolePermission : Entity, IMustHaveTenant
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public Guid TenantId { get; private set; }

    public RolePermission(Guid roleId, Guid permissionId, Guid tenantId) : base()
    {
        RoleId = roleId;
        PermissionId = permissionId;
        TenantId = tenantId;
    }

    private RolePermission()
    {
    }
}
