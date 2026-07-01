using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Billing;

/// <summary>
/// Represents a specific payment channel under a provider
/// (e.g., BCA Virtual Account under Xendit, OVO under Xendit).
/// Manageable entity — can be activated/deactivated from admin panel.
/// </summary>
public class PaymentChannel : Entity
{
    public Guid PaymentProviderId { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public PaymentMethodType Type { get; private set; }
    public string? LogoUrl { get; private set; }
    public decimal? MinTransactionAmount { get; private set; }
    public decimal? MaxTransactionAmount { get; private set; }
    public decimal? FeeFlat { get; private set; }
    public decimal? FeePercent { get; private set; }
    public bool IsActive { get; private set; }

    public PaymentChannel(
        Guid id,
        Guid paymentProviderId,
        string name,
        string code,
        PaymentMethodType type,
        string? logoUrl = null,
        decimal? minTransactionAmount = null,
        decimal? maxTransactionAmount = null,
        decimal? feeFlat = null,
        decimal? feePercent = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Channel name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Channel code is required.", nameof(code));

        PaymentProviderId = paymentProviderId;
        Name = name;
        Code = code.ToUpperInvariant();
        Type = type;
        LogoUrl = logoUrl;
        MinTransactionAmount = minTransactionAmount;
        MaxTransactionAmount = maxTransactionAmount;
        FeeFlat = feeFlat;
        FeePercent = feePercent;
        IsActive = true;
    }

    // EF Core Constructor
    private PaymentChannel()
    {
        Name = null!;
        Code = null!;
    }

    public void UpdateDetails(
        string name,
        string? logoUrl,
        decimal? minTransactionAmount,
        decimal? maxTransactionAmount,
        decimal? feeFlat,
        decimal? feePercent)
    {
        Name = name;
        LogoUrl = logoUrl;
        MinTransactionAmount = minTransactionAmount;
        MaxTransactionAmount = maxTransactionAmount;
        FeeFlat = feeFlat;
        FeePercent = feePercent;
    }

    /// <summary>
    /// Validates whether the given amount is within this channel's allowed range.
    /// </summary>
    public bool IsAmountAllowed(Money amount)
    {
        if (MinTransactionAmount.HasValue && amount.Amount < MinTransactionAmount.Value) return false;
        if (MaxTransactionAmount.HasValue && amount.Amount > MaxTransactionAmount.Value) return false;
        return true;
    }

    /// <summary>
    /// Calculates the fee for a given transaction amount.
    /// </summary>
    public Money CalculateFee(Money amount)
    {
        var flat = FeeFlat ?? 0;
        var percent = FeePercent.HasValue ? amount.Amount * (FeePercent.Value / 100m) : 0;
        return new Money(flat + percent, amount.CurrencyCode);
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
