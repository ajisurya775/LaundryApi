using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Catalog;

public class PriceList : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } // e.g. Regular, Member, Corporate, Branch A
    public string Description { get; private set; }
    public bool IsActive { get; private set; }

    public PriceList(Guid id, Guid tenantId, string name, string description) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        IsActive = true;
    }

    private PriceList()
    {
        Name = null!;
        Description = null!;
    }
}
