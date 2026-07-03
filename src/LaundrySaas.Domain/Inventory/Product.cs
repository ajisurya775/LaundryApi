using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Inventory;

public class Product : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } // e.g. Detergent, Softener
    public string Sku { get; private set; }
    public bool IsActive { get; private set; }

    public Product(Guid id, Guid tenantId, string name, string sku) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Sku = sku;
        IsActive = true;
    }

    private Product()
    {
        Name = null!;
        Sku = null!;
    }
}
