using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Inventory;

public class Purchase : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string OrderNumber { get; private set; }
    public Money TotalCost { get; private set; }
    public DateTime PurchaseDate { get; private set; }

    public Purchase(Guid id, Guid tenantId, string orderNumber, Money totalCost, DateTime purchaseDate) : base(id)
    {
        TenantId = tenantId;
        OrderNumber = orderNumber;
        TotalCost = totalCost;
        PurchaseDate = purchaseDate;
    }

    private Purchase()
    {
        OrderNumber = null!;
        TotalCost = null!;
    }
}
