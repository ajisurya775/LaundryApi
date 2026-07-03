using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Organization;

public class Branch : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string PhoneNumber { get; private set; }
    public bool IsActive { get; private set; }

    public Branch(Guid id, Guid tenantId, string name, string address, string phoneNumber) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Address = address;
        PhoneNumber = phoneNumber;
        IsActive = true;
    }

    private Branch()
    {
        Name = null!;
        Address = null!;
        PhoneNumber = null!;
    }

    public void UpdateDetails(string name, string address, string phoneNumber)
    {
        Name = name;
        Address = address;
        PhoneNumber = phoneNumber;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
