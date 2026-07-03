using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Customer;

public class Point : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public int Balance { get; private set; }

    public Point(Guid id, Guid tenantId, Guid customerId, int balance) : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        Balance = balance;
    }

    private Point()
    {
    }
}
