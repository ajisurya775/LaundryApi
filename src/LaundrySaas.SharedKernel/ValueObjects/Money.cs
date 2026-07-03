using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.SharedKernel.ValueObjects;

public class Money : ValueObject, IComparable<Money>, IComparable
{
    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; }

    public Currency Currency => Currency.From(CurrencyCode);

    // EF Core Constructor
    private Money()
    {
        Amount = 0;
        CurrencyCode = "IDR";
    }

    public Money(decimal amount, Currency currency)
    {
        if (currency == null) throw new DomainException("Currency is required.");
        
        // Ensure scale is at most 2
        int scale = BitConverter.GetBytes(decimal.GetBits(amount)[3])[2];
        if (scale > 2)
        {
            throw new DomainException($"Amount {amount} has invalid precision. Max 2 decimal places allowed.");
        }

        Amount = amount;
        CurrencyCode = currency.Code;
    }

    public static Money From(decimal amount, Currency currency)
    {
        var roundedAmount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        return new Money(roundedAmount, currency);
    }

    public static Money From(decimal amount, string currencyCode = "IDR")
    {
        var currency = Currency.From(currencyCode);
        return From(amount, currency);
    }

    // Currency-specific Factories
    public static Money IDR(decimal amount) => From(amount, Currency.IDR);
    public static Money USD(decimal amount) => From(amount, Currency.USD);
    public static Money MYR(decimal amount) => From(amount, Currency.MYR);
    public static Money SGD(decimal amount) => From(amount, Currency.SGD);

    public static Money Zero(Currency currency) => new(0, currency);
    public static Money Zero(string currencyCode = "IDR") => new(0, Currency.From(currencyCode));

    // Minor unit conversion
    public static Money FromMinorUnit(long minorAmount, Currency currency)
    {
        var divisor = (decimal)Math.Pow(10, currency.Exponent);
        var amount = minorAmount / divisor;
        return new Money(amount, currency);
    }

    public static Money FromMinorUnit(long minorAmount, string currencyCode)
    {
        var currency = Currency.From(currencyCode);
        return FromMinorUnit(minorAmount, currency);
    }

    public long ToMinorUnit()
    {
        var multiplier = (decimal)Math.Pow(10, Currency.Exponent);
        return (long)Math.Round(Amount * multiplier, 0, MidpointRounding.AwayFromZero);
    }

    // Parsing
    public static Money Parse(string value, string currencyCode = "IDR")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Value cannot be null or empty.");

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
        {
            return From(amount, currencyCode);
        }

        throw new DomainException($"Invalid money format: '{value}'");
    }

    public bool IsZero => Amount == 0;
    public bool IsNegative => Amount < 0;

    public Money Abs() => new(Math.Abs(Amount), Currency);

    public Money Round() => new(Math.Round(Amount, 2, MidpointRounding.AwayFromZero), Currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal multiplier)
    {
        var result = Amount * multiplier;
        var rounded = Math.Round(result, 2, MidpointRounding.AwayFromZero);
        return new Money(rounded, Currency);
    }

    public Money Divide(decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Cannot divide money by zero.");
        var result = Amount / divisor;
        var rounded = Math.Round(result, 2, MidpointRounding.AwayFromZero);
        return new Money(rounded, Currency);
    }

    // Allocation
    public Money[] Allocate(int parts)
    {
        if (parts <= 0)
            throw new ArgumentException("Parts must be greater than zero.", nameof(parts));

        var cents = Amount * 100;
        var partAmount = Math.Floor(cents / parts);
        var remainder = (int)(cents % parts);

        var result = new Money[parts];
        for (int i = 0; i < parts; i++)
        {
            var amount = partAmount + (i < remainder ? 1 : 0);
            result[i] = new Money(amount / 100m, Currency);
        }

        return result;
    }

    public Money[] Allocate(decimal[] ratios)
    {
        if (ratios == null || ratios.Length == 0)
            throw new ArgumentException("Ratios cannot be empty.", nameof(ratios));

        var totalRatio = ratios.Sum();
        if (totalRatio <= 0)
            throw new ArgumentException("Sum of ratios must be greater than zero.", nameof(ratios));

        var cents = (int)Math.Round(Amount * 100, 0, MidpointRounding.AwayFromZero);
        var remainder = cents;
        var results = new int[ratios.Length];

        for (int i = 0; i < ratios.Length; i++)
        {
            var share = (int)Math.Floor(cents * ratios[i] / totalRatio);
            results[i] = share;
            remainder -= share;
        }

        // Distribute remainder (1 cent at a time)
        for (int i = 0; remainder > 0; i = (i + 1) % ratios.Length)
        {
            results[i]++;
            remainder--;
        }

        return results.Select(r => new Money(r / 100m, Currency)).ToArray();
    }

    // Operators
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal multiplier) => money.Multiply(multiplier);
    public static Money operator *(decimal multiplier, Money money) => money.Multiply(multiplier);
    public static Money operator /(Money money, decimal divisor) => money.Divide(divisor);

    public override bool Equals(object? obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();

    public static bool operator ==(Money? left, Money? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Money? left, Money? right) => !(left == right);

    public static bool operator <(Money left, Money right)
    {
        left.EnsureSameCurrency(right);
        return left.Amount < right.Amount;
    }

    public static bool operator >(Money left, Money right)
    {
        left.EnsureSameCurrency(right);
        return left.Amount > right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        left.EnsureSameCurrency(right);
        return left.Amount <= right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        left.EnsureSameCurrency(right);
        return left.Amount >= right.Amount;
    }

    private void EnsureSameCurrency(Money other)
    {
        if (CurrencyCode != other.CurrencyCode)
            throw new InvalidOperationException($"Currency mismatch: {CurrencyCode} vs {other.CurrencyCode}");
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return CurrencyCode;
    }

    public int CompareTo(Money? other)
    {
        if (other is null) return 1;
        EnsureSameCurrency(other);
        return Amount.CompareTo(other.Amount);
    }

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (obj is not Money other) throw new ArgumentException("Object must be of type Money.", nameof(obj));
        return CompareTo(other);
    }

    public override string ToString()
    {
        if (CurrencyCode == "IDR")
            return $"Rp{Amount:N0}";
        if (CurrencyCode == "USD")
            return $"${Amount:N2}";
        if (CurrencyCode == "MYR")
            return $"RM{Amount:N2}";
        if (CurrencyCode == "SGD")
            return $"S${Amount:N2}";
        
        return $"{CurrencyCode} {Amount:N2}";
    }
}
