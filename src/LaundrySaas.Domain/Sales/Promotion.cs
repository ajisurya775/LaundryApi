using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Sales;

public class Promotion : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    public Promotion(Guid id, Guid tenantId, string code, string name) : base(id)
    {
        TenantId = tenantId;
        Code = code;
        Name = name;
        IsActive = true;
    }

    private Promotion()
    {
        Code = null!;
        Name = null!;
    }
}
