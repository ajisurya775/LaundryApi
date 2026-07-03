using System;
using System.Collections.Generic;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.SharedKernel.ValueObjects;

public class Weight : ValueObject
{
    public decimal Value { get; }

    private Weight()
    {
        Value = 0;
    }

    public Weight(decimal value)
    {
        if (value < 0)
        {
            throw new DomainException("Weight cannot be negative.");
        }

        Value = Math.Round(value, 3, MidpointRounding.AwayFromZero);
    }

    public static Weight From(decimal value) => new(value);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value:N3} Kg";
}
