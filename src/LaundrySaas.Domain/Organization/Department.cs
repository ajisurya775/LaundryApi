using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Organization;

public class Department : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    public Department(Guid id, Guid tenantId, Guid branchId, string name) : base(id)
    {
        TenantId = tenantId;
        BranchId = branchId;
        Name = name;
        IsActive = true;
    }

    private Department()
    {
        Name = null!;
    }
}
