using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Sales;

public class Invoice : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid OrderId { get; private set; }
    public string Number { get; private set; }
    public Money Total { get; private set; }
    public DateTime IssuedAt { get; private set; }

    public Invoice(Guid id, Guid tenantId, Guid orderId, string number, Money total, DateTime issuedAt) : base(id)
    {
        TenantId = tenantId;
        OrderId = orderId;
        Number = number;
        Total = total;
        IssuedAt = issuedAt;
    }

    private Invoice()
    {
        Number = null!;
        Total = null!;
    }
}
