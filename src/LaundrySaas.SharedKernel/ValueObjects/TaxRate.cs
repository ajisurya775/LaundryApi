using System;
using System.Collections.Generic;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.SharedKernel.ValueObjects;

public class TaxRate : ValueObject
{
    public decimal Rate { get; }

    private TaxRate()
    {
        Rate = 0;
    }

    public TaxRate(decimal rate)
    {
        if (rate < 0 || rate > 100)
        {
            throw new DomainException($"Tax rate {rate} must be between 0 and 100.");
        }

        Rate = Math.Round(rate, 2, MidpointRounding.AwayFromZero);
    }

    public static TaxRate From(decimal rate) => new(rate);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Rate;
    }

    public override string ToString() => $"{Rate}%";
}
