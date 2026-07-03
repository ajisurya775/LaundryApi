using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.POS;

public class CashMovement : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid ShiftId { get; private set; }
    public string Type { get; private set; } // e.g. Add, Deduct, Setor
    public Money Amount { get; private set; }
    public string Reason { get; private set; }

    public CashMovement(Guid id, Guid tenantId, Guid shiftId, string type, Money amount, string reason) : base(id)
    {
        TenantId = tenantId;
        ShiftId = shiftId;
        Type = type;
        Amount = amount;
        Reason = reason;
    }

    private CashMovement()
    {
        Type = null!;
        Amount = null!;
        Reason = null!;
    }
}
