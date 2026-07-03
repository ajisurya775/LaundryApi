using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Catalog;

public class TaxProfile : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public TaxRate Rate { get; private set; }
    public bool IsActive { get; private set; }

    public TaxProfile(Guid id, Guid tenantId, string name, TaxRate rate) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Rate = rate;
        IsActive = true;
    }

    private TaxProfile()
    {
        Name = null!;
        Rate = null!;
    }
}
