using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.POS;

public class CashDrawer : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid PosId { get; private set; }
    public Money Balance { get; private set; }
    public bool IsActive { get; private set; }

    public CashDrawer(Guid id, Guid tenantId, Guid posId, Money balance) : base(id)
    {
        TenantId = tenantId;
        PosId = posId;
        Balance = balance;
        IsActive = true;
    }

    private CashDrawer()
    {
        Balance = null!;
    }
}
