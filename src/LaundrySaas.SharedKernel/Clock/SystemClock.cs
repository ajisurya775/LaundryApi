using System;

namespace LaundrySaas.SharedKernel.Clock;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
