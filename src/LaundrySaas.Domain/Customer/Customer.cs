using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Customer;

public class Customer : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public bool IsActive { get; private set; }

    public Customer(Guid id, Guid tenantId, string name, string email, string phoneNumber) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        IsActive = true;
    }

    private Customer()
    {
        Name = null!;
        Email = null!;
        PhoneNumber = null!;
    }
}
