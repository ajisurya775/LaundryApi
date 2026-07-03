using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Catalog;

public class PriceRule : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid PriceListId { get; private set; }
    public Guid ServiceVariantId { get; private set; }
    public bool IsActive { get; private set; }

    public PriceRule(Guid id, Guid tenantId, Guid priceListId, Guid serviceVariantId) : base(id)
    {
        TenantId = tenantId;
        PriceListId = priceListId;
        ServiceVariantId = serviceVariantId;
        IsActive = true;
    }

    private PriceRule()
    {
    }
}
