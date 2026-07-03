using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Billing;

public class Subscription : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid PlanId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Status { get; private set; } // Active, Expired, Cancelled

    public Subscription(Guid id, Guid tenantId, Guid planId, DateTime startDate, DateTime endDate, string status = "Active") : base(id)
    {
        TenantId = tenantId;
        PlanId = planId;
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
    }

    private Subscription()
    {
        Status = null!;
    }
}
