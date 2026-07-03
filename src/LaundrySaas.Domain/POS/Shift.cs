using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.POS;

public class Shift : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid PosId { get; private set; }
    public Guid OpenedBy { get; private set; }
    public Guid? ClosedBy { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public Money OpeningBalance { get; private set; }
    public Money? ClosingBalance { get; private set; }
    public string Status { get; private set; } // "Open" or "Closed"

    public Shift(Guid id, Guid tenantId, Guid posId, Guid openedBy, Money openingBalance) : base(id)
    {
        TenantId = tenantId;
        PosId = posId;
        OpenedBy = openedBy;
        OpeningBalance = openingBalance;
        OpenedAt = DateTime.UtcNow;
        Status = "Open";
    }

    private Shift()
    {
        Status = null!;
        OpeningBalance = null!;
    }

    public void Close(Guid closedBy, Money closingBalance)
    {
        if (Status == "Closed")
            throw new InvalidOperationException("Shift is already closed.");

        if (OpeningBalance.CurrencyCode != closingBalance.CurrencyCode)
            throw new InvalidOperationException("Closing balance currency must match opening balance currency.");

        ClosedBy = closedBy;
        ClosingBalance = closingBalance;
        ClosedAt = DateTime.UtcNow;
        Status = "Closed";
    }
}
