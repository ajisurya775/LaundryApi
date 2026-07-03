using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Customer;

public class CustomerGroup : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    public CustomerGroup(Guid id, Guid tenantId, string name) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        IsActive = true;
    }

    private CustomerGroup()
    {
        Name = null!;
    }
}
