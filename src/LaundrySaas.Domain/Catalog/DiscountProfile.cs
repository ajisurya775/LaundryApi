using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Catalog;

public class DiscountProfile : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public Percentage Value { get; private set; }
    public bool IsActive { get; private set; }

    public DiscountProfile(Guid id, Guid tenantId, string name, Percentage value) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Value = value;
        IsActive = true;
    }

    private DiscountProfile()
    {
        Name = null!;
        Value = null!;
    }
}
