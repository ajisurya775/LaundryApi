using System;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Organization;

public class Tenant : AggregateRoot
{
    public string Name { get; private set; }
    public string CompanyName { get; private set; }
    public string? CountryCode { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool IsActive { get; private set; }

    public Tenant(Guid id, string name, string companyName, string? countryCode, string? phoneNumber) : base(id)
    {
        Name = name;
        CompanyName = companyName;
        CountryCode = countryCode;
        PhoneNumber = phoneNumber;
        IsActive = true;
    }

    private Tenant()
    {
        Name = null!;
        CompanyName = null!;
    }

    public void UpdateDetails(string name, string companyName, string? countryCode, string? phoneNumber)
    {
        Name = name;
        CompanyName = companyName;
        CountryCode = countryCode;
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
