using LaundrySaas.SharedKernel.Interfaces;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.MultiTenancy;

public class Pos : Entity, IMustHaveTenant
{
    private readonly List<PosPaymentMethod> _paymentMethods = new();

    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<PosPaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

    public Pos(Guid id, Guid tenantId, Guid branchId, string name) : base(id)
    {
        TenantId = tenantId;
        BranchId = branchId;
        Name = name;
        IsActive = true;
    }

    // EF Core Constructor
    private Pos()
    {
        Name = null!;
    }

    public void UpdateName(string name)
    {
        Name = name;
    }

    public void AddPaymentMethod(Guid paymentMethodId)
    {
        if (!_paymentMethods.Any(pm => pm.PaymentMethodId == paymentMethodId))
        {
            _paymentMethods.Add(new PosPaymentMethod(Id, paymentMethodId, TenantId));
        }
    }

    public void RemovePaymentMethod(Guid paymentMethodId)
    {
        var existing = _paymentMethods.FirstOrDefault(pm => pm.PaymentMethodId == paymentMethodId);
        if (existing is not null)
        {
            _paymentMethods.Remove(existing);
        }
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
