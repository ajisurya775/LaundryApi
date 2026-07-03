using System;
using System.Collections.Generic;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.SharedKernel.ValueObjects;

public class Currency : ValueObject
{
    private static readonly Dictionary<string, int> ValidCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "IDR", 0 }, // Indonesian Rupiah uses exponent 0 practically
        { "USD", 2 },
        { "MYR", 2 },
        { "SGD", 2 },
        { "EUR", 2 },
        { "GBP", 2 },
        { "AUD", 2 },
        { "CAD", 2 },
        { "JPY", 0 },
        { "CNY", 2 },
        { "VND", 0 }
    };

    public string Code { get; private set; }
    public int Exponent { get; private set; }

    private Currency(string code, int exponent)
    {
        Code = code.ToUpperInvariant();
        Exponent = exponent;
    }

    private Currency()
    {
        Code = null!;
    }

    public static Currency From(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Currency code cannot be empty.");

        var upperCode = code.ToUpperInvariant();
        if (!ValidCodes.TryGetValue(upperCode, out int exponent))
        {
            // Default to exponent 2 for unknown currencies
            return new Currency(upperCode, 2);
        }

        return new Currency(upperCode, exponent);
    }

    public static readonly Currency IDR = new("IDR", 0);
    public static readonly Currency USD = new("USD", 2);
    public static readonly Currency MYR = new("MYR", 2);
    public static readonly Currency SGD = new("SGD", 2);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
        yield return Exponent;
    }

    public override string ToString() => Code;
}
