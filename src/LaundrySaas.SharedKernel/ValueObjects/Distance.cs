using System;
using System.Collections.Generic;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.SharedKernel.ValueObjects;

public class Distance : ValueObject
{
    public decimal Meters { get; }

    public decimal Kilometers => Meters / 1000m;

    private Distance()
    {
        Meters = 0;
    }

    public Distance(decimal meters)
    {
        if (meters < 0)
        {
            throw new DomainException("Distance cannot be negative.");
        }

        Meters = Math.Round(meters, 2, MidpointRounding.AwayFromZero);
    }

    public static Distance FromMeters(decimal meters) => new(meters);
    public static Distance FromKilometers(decimal kilometers) => new(kilometers * 1000m);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Meters;
    }

    public override string ToString() => Meters < 1000 ? $"{Meters:N0} m" : $"{Kilometers:N2} km";
}
