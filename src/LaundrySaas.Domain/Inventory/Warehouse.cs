using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Inventory;

public class Warehouse : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    public Warehouse(Guid id, Guid tenantId, string name) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        IsActive = true;
    }

    private Warehouse()
    {
        Name = null!;
    }
}
