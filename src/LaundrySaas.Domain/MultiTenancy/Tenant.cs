using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.MultiTenancy;

public class Tenant : AggregateRoot
{
    public string Name { get; private set; }
    public string CompanyName { get; private set; }
    public bool IsActive { get; private set; }

    public Tenant(Guid id, string name, string companyName) : base(id)
    {
        Name = name;
        CompanyName = companyName;
        IsActive = true;
    }

    // EF Core Constructor
    private Tenant()
    {
        Name = null!;
        CompanyName = null!;
    }

    public void UpdateDetails(string name, string companyName)
    {
        Name = name;
        CompanyName = companyName;
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
