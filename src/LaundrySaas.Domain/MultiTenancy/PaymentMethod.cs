using LaundrySaas.SharedKernel.Interfaces;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.MultiTenancy;

public class PaymentMethod : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    public PaymentMethod(Guid id, Guid tenantId, string name) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        IsActive = true;
    }

    // EF Core Constructor
    private PaymentMethod()
    {
        Name = null!;
    }

    public void UpdateName(string name)
    {
        Name = name;
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
