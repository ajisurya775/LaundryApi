using System;
using System.Collections.Generic;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.SharedKernel.ValueObjects;

public class Percentage : ValueObject
{
    public decimal Value { get; }

    private Percentage()
    {
        Value = 0;
    }

    public Percentage(decimal value)
    {
        if (value < 0 || value > 100)
        {
            throw new DomainException($"Percentage value {value} must be between 0 and 100.");
        }

        Value = Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    public static Percentage From(decimal value) => new(value);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value}%";
}
