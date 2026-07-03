using System;

namespace LaundrySaas.SharedKernel.Identity;

public static class Ulid
{
    public static Guid NewUlid()
    {
        return SequentialGuid.NewGuid();
    }
}
