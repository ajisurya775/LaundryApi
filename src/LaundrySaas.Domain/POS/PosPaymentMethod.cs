using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.POS;

public class PosPaymentMethod : Entity, IMustHaveTenant
{
    public Guid PosId { get; private set; }
    public Guid PaymentMethodId { get; private set; }
    public Guid TenantId { get; private set; }

    public PosPaymentMethod(Guid posId, Guid paymentMethodId, Guid tenantId) : base()
    {
        PosId = posId;
        PaymentMethodId = paymentMethodId;
        TenantId = tenantId;
    }

    private PosPaymentMethod()
    {
    }
}
