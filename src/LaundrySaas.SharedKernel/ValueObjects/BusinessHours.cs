using System;
using System.Collections.Generic;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.SharedKernel.ValueObjects;

public class BusinessHours : ValueObject
{
    public TimeSpan OpenTime { get; }
    public TimeSpan CloseTime { get; }

    private BusinessHours()
    {
        OpenTime = TimeSpan.Zero;
        CloseTime = TimeSpan.Zero;
    }

    public BusinessHours(TimeSpan openTime, TimeSpan closeTime)
    {
        if (openTime < TimeSpan.Zero || openTime >= TimeSpan.FromHours(24))
            throw new DomainException("Open time must be between 00:00 and 23:59.");
        
        if (closeTime < TimeSpan.Zero || closeTime >= TimeSpan.FromHours(24))
            throw new DomainException("Close time must be between 00:00 and 23:59.");

        if (closeTime < openTime)
            throw new DomainException("Close time cannot be before open time.");

        OpenTime = openTime;
        CloseTime = closeTime;
    }

    public bool IsOpen(TimeSpan time)
    {
        return time >= OpenTime && time <= CloseTime;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return OpenTime;
        yield return CloseTime;
    }

    public override string ToString() => $"{OpenTime:hh\\:mm} - {CloseTime:hh\\:mm}";
}
