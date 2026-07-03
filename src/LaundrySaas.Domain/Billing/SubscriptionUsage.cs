using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Billing;

public class SubscriptionUsage : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public string FeatureName { get; private set; } // e.g. MaxBranches, MaxUsers
    public Quantity UsedAmount { get; private set; }

    public SubscriptionUsage(Guid id, Guid tenantId, Guid subscriptionId, string featureName, Quantity usedAmount) : base(id)
    {
        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        FeatureName = featureName;
        UsedAmount = usedAmount;
    }

    private SubscriptionUsage()
    {
        FeatureName = null!;
        UsedAmount = null!;
    }
}
