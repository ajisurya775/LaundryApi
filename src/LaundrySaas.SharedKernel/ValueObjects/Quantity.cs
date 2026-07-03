using System;
using System.Collections.Generic;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.SharedKernel.ValueObjects;

public class Quantity : ValueObject
{
    public decimal Value { get; }

    private Quantity()
    {
        Value = 0;
    }

    public Quantity(decimal value)
    {
        if (value < 0)
        {
            throw new DomainException("Quantity cannot be negative.");
        }

        Value = Math.Round(value, 3, MidpointRounding.AwayFromZero);
    }

    public static Quantity From(decimal value) => new(value);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString("G");
}
