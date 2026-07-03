using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Customer;

public class Voucher : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public Money Value { get; private set; }
    public bool IsActive { get; private set; }

    public Voucher(Guid id, Guid tenantId, string code, Money value) : base(id)
    {
        TenantId = tenantId;
        Code = code;
        Value = value;
        IsActive = true;
    }

    private Voucher()
    {
        Code = null!;
        Value = null!;
    }
}
