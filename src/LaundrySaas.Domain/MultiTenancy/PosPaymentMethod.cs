using LaundrySaas.SharedKernel.Interfaces;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.MultiTenancy;

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

    // EF Core Constructor
    private PosPaymentMethod()
    {
    }
}
