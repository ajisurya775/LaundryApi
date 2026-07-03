using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Customer;

public class Membership : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Tier { get; private set; } // e.g. Gold, Silver, Platinum
    public bool IsActive { get; private set; }

    public Membership(Guid id, Guid tenantId, Guid customerId, string tier) : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        Tier = tier;
        IsActive = true;
    }

    private Membership()
    {
        Tier = null!;
    }
}
