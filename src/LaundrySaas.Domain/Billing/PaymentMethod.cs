using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Billing;

public class PaymentMethod : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public PaymentMethodType Type { get; private set; }
    public bool IsActive { get; private set; }
    public string? AssetUrl { get; private set; }
    public decimal MinTransaction { get; private set; }
    public decimal MaxTransaction { get; private set; }

    public PaymentMethod(
        Guid id,
        Guid tenantId,
        string name,
        PaymentMethodType type,
        string? assetUrl = null,
        decimal minTransaction = 0,
        decimal maxTransaction = 0) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Payment method name is required.", nameof(name));

        TenantId = tenantId;
        Name = name;
        Type = type;
        IsActive = true;
        AssetUrl = assetUrl;
        MinTransaction = minTransaction;
        MaxTransaction = maxTransaction;
    }

    private PaymentMethod()
    {
        Name = null!;
    }

    public void UpdateName(string name)
    {
        Name = name;
    }

    public void UpdateType(PaymentMethodType type)
    {
        Type = type;
    }

    public void UpdateAssetUrl(string? assetUrl)
    {
        AssetUrl = assetUrl;
    }

    public void UpdateTransactionLimits(decimal min, decimal max)
    {
        if (min < 0)
            throw new ArgumentException("Min transaction cannot be negative.", nameof(min));

        if (max < 0)
            throw new ArgumentException("Max transaction cannot be negative.", nameof(max));

        if (max > 0 && min > max)
            throw new ArgumentException("Min transaction cannot be greater than max.", nameof(min));

        MinTransaction = min;
        MaxTransaction = max;
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
