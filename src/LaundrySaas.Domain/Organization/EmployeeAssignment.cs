using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Organization;

public class EmployeeAssignment : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? BranchId { get; private set; } // Null if tenant-wide assignment
    public Guid? DepartmentId { get; private set; }
    public Guid? PosId { get; private set; } // Null if not assigned to specific POS
    public Guid RoleId { get; private set; }
    public string EmploymentType { get; private set; } // e.g. Permanent, Contract, PartTime
    public string Status { get; private set; } // e.g. Active, Suspended
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveUntil { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public int Priority { get; private set; } // Higher priority overrides overlapping roles
    public string? Remarks { get; private set; }

    public EmployeeAssignment(
        Guid id,
        Guid tenantId,
        Guid userId,
        Guid? branchId,
        Guid? departmentId,
        Guid? posId,
        Guid roleId,
        string employmentType = "Permanent",
        string status = "Active",
        DateTime? effectiveFrom = null,
        DateTime? effectiveUntil = null,
        bool isDefault = false,
        int priority = 0,
        string? remarks = null) : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        BranchId = branchId;
        DepartmentId = departmentId;
        PosId = posId;
        RoleId = roleId;
        EmploymentType = employmentType;
        Status = status;
        EffectiveFrom = effectiveFrom ?? DateTime.UtcNow;
        EffectiveUntil = effectiveUntil;
        IsDefault = isDefault;
        IsActive = true;
        Priority = priority;
        Remarks = remarks;
    }

    private EmployeeAssignment()
    {
        EmploymentType = null!;
        Status = null!;
    }

    public void UnsetDefault()
    {
        IsDefault = false;
    }

    public void SetDefault()
    {
        IsDefault = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public bool IsCurrentlyActive()
    {
        if (!IsActive)
            return false;

        var now = DateTime.UtcNow;
        if (now < EffectiveFrom)
            return false;

        if (EffectiveUntil.HasValue && now > EffectiveUntil.Value)
            return false;

        return Status.Equals("Active", StringComparison.OrdinalIgnoreCase);
    }
}
