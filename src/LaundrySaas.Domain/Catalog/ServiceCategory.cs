using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Catalog;

public class ServiceCategory : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    public ServiceCategory(Guid id, Guid tenantId, string name) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        IsActive = true;
    }

    private ServiceCategory()
    {
        Name = null!;
    }
}
