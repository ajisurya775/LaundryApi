using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Inventory;

public class Adjustment : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid ProductId { get; private set; }
    public Quantity AdjustedQuantity { get; private set; }
    public string Reason { get; private set; }

    public Adjustment(Guid id, Guid tenantId, Guid warehouseId, Guid productId, Quantity adjustedQuantity, string reason) : base(id)
    {
        TenantId = tenantId;
        WarehouseId = warehouseId;
        ProductId = productId;
        AdjustedQuantity = adjustedQuantity;
        Reason = reason;
    }

    private Adjustment()
    {
        AdjustedQuantity = null!;
        Reason = null!;
    }
}
