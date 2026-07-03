using System;
using System.Collections.Generic;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.SharedKernel.ValueObjects;

public class DateRange : ValueObject
{
    public DateTime Start { get; }
    public DateTime End { get; }

    public TimeSpan Duration => End - Start;

    private DateRange()
    {
        Start = DateTime.MinValue;
        End = DateTime.MaxValue;
    }

    public DateRange(DateTime start, DateTime end)
    {
        if (end < start)
        {
            throw new DomainException("End date cannot be before start date.");
        }

        Start = start;
        End = end;
    }

    public bool OverlapsWith(DateRange other)
    {
        if (other is null) return false;
        return Start < other.End && End > other.Start;
    }

    public bool Contains(DateTime dateTime)
    {
        return dateTime >= Start && dateTime <= End;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Start;
        yield return End;
    }

    public override string ToString() => $"{Start:yyyy-MM-dd} to {End:yyyy-MM-dd}";
}
