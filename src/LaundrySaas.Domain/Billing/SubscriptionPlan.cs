using System;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Billing;

public class SubscriptionPlan : Entity
{
    public string Name { get; private set; } // e.g. Free, Basic, Pro, Enterprise
    public Money Price { get; private set; }
    public bool HasPos { get; private set; }
    public bool HasInventory { get; private set; }
    public bool HasAccounting { get; private set; }
    public decimal ExtraCredit { get; private set; }
    public bool IsActive { get; private set; }

    public SubscriptionPlan(
        Guid id, 
        string name, 
        Money price,
        bool hasPos = false,
        bool hasInventory = false,
        bool hasAccounting = false,
        decimal extraCredit = 0) : base(id)
    {
        Name = name;
        Price = price;
        HasPos = hasPos;
        HasInventory = hasInventory;
        HasAccounting = hasAccounting;
        ExtraCredit = extraCredit;
        IsActive = true;
    }

    private SubscriptionPlan()
    {
        Name = null!;
        Price = null!;
    }
}
