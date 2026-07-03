using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Catalog;

public class Service : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public bool IsActive { get; private set; }

    public Service(Guid id, Guid tenantId, string name, string description, Guid categoryId) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        CategoryId = categoryId;
        IsActive = true;
    }

    private Service()
    {
        Name = null!;
        Description = null!;
    }
}
