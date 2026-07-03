using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Inventory;

public class StockMovement : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid SourceWarehouseId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public Quantity Quantity { get; private set; }
    public string Reason { get; private set; }

    public StockMovement(Guid id, Guid tenantId, Guid productId, Guid sourceWarehouseId, Guid destinationWarehouseId, Quantity quantity, string reason) : base(id)
    {
        TenantId = tenantId;
        ProductId = productId;
        SourceWarehouseId = sourceWarehouseId;
        DestinationWarehouseId = destinationWarehouseId;
        Quantity = quantity;
        Reason = reason;
    }

    private StockMovement()
    {
        Quantity = null!;
        Reason = null!;
    }
}
