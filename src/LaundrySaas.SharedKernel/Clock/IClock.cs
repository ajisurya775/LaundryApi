using System;

namespace LaundrySaas.SharedKernel.Clock;

public interface IClock
{
    DateTime UtcNow { get; }
}
