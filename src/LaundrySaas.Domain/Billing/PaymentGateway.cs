using System;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Billing;

public class PaymentGateway : Entity
{
    public string Name { get; private set; }
    public string Code { get; private set; } // e.g. MIDTRANS, XENDIT, STRIPE, MANUAL
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public PaymentGateway(Guid id, string name, string code, string? description = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Gateway name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Gateway code is required.", nameof(code));

        Name = name;
        Code = code.ToUpperInvariant();
        Description = description;
        IsActive = true;
    }

    private PaymentGateway()
    {
        Name = null!;
        Code = null!;
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
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
