using System;
using System.Collections.Generic;
using System.Linq;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Identity;

public class Role : AggregateRoot, IMustHaveTenant
{
    private readonly List<RolePermission> _rolePermissions = new();

    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; } // e.g. OWNER, MANAGER, CASHIER, ACCOUNTANT
    public string? Description { get; private set; }
    public int Level { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    public Role(Guid id, Guid tenantId, string name, string code, int level, string? description = null, bool isSystem = false) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Role code is required.", nameof(code));

        TenantId = tenantId;
        Name = name;
        Code = code.ToUpperInvariant();
        Level = level;
        Description = description;
        IsSystem = isSystem;
        IsActive = true;
    }

    private Role()
    {
        Name = null!;
        Code = null!;
    }

    public void UpdateDetails(string name, string code, int level, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Role code is required.", nameof(code));

        Name = name;
        Code = code.ToUpperInvariant();
        Level = level;
        Description = description;
    }

    public void AddPermission(Guid permissionId)
    {
        if (!_rolePermissions.Any(rp => rp.PermissionId == permissionId))
        {
            _rolePermissions.Add(new RolePermission(Id, permissionId, TenantId));
        }
    }

    public void RemovePermission(Guid permissionId)
    {
        var existing = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (existing is not null)
        {
            _rolePermissions.Remove(existing);
        }
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
