using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Inventory;

public class Stock : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid ProductId { get; private set; }
    public Quantity CurrentStock { get; private set; }

    public Stock(Guid id, Guid tenantId, Guid warehouseId, Guid productId, Quantity currentStock) : base(id)
    {
        TenantId = tenantId;
        WarehouseId = warehouseId;
        ProductId = productId;
        CurrentStock = currentStock;
    }

    private Stock()
    {
        CurrentStock = null!;
    }
}
