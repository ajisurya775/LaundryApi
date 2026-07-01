using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Billing;

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string CurrencyCode { get; }

    public Money(decimal amount, string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code cannot be empty.", nameof(currencyCode));

        Amount = amount;
        CurrencyCode = currencyCode.ToUpperInvariant();
    }

    public static Money IDR(decimal amount) => new(amount, "IDR");

    public static Money Zero(string currencyCode = "IDR") => new(0, currencyCode);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, CurrencyCode);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, CurrencyCode);
    }

    public bool IsNegative() => Amount < 0;

    public bool IsZero() => Amount == 0;

    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    public bool IsGreaterThanOrEqual(Money other)
    {
        EnsureSameCurrency(other);
        return Amount >= other.Amount;
    }

    public bool IsLessThanOrEqual(Money other)
    {
        EnsureSameCurrency(other);
        return Amount <= other.Amount;
    }

    private void EnsureSameCurrency(Money other)
    {
        if (CurrencyCode != other.CurrencyCode)
            throw new InvalidOperationException(
                $"Cannot operate on different currencies: {CurrencyCode} vs {other.CurrencyCode}.");
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return CurrencyCode;
    }

    public override string ToString() => $"{CurrencyCode} {Amount:N0}";
}
