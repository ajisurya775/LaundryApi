using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Catalog;

public class ServiceVariant : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid ServiceId { get; private set; }
    public string VariantName { get; private set; } // e.g. Express, Regular, 5 Kg, 10 Kg
    public bool IsActive { get; private set; }

    public ServiceVariant(Guid id, Guid tenantId, Guid serviceId, string variantName) : base(id)
    {
        TenantId = tenantId;
        ServiceId = serviceId;
        VariantName = variantName;
        IsActive = true;
    }

    private ServiceVariant()
    {
        VariantName = null!;
    }
}
