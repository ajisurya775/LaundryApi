using System;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.SharedKernel.Common;

public static class Guard
{
    public static void AgainstNullOrEmpty(string? value, string name, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(message ?? $"{name} cannot be null or empty.");
        }
    }

    public static void AgainstNull(object? value, string name, string? message = null)
    {
        if (value is null)
        {
            throw new DomainException(message ?? $"{name} cannot be null.");
        }
    }

    public static void AgainstNegative(decimal value, string name, string? message = null)
    {
        if (value < 0)
        {
            throw new DomainException(message ?? $"{name} cannot be negative.");
        }
    }

    public static void AgainstNegativeOrZero(decimal value, string name, string? message = null)
    {
        if (value <= 0)
        {
            throw new DomainException(message ?? $"{name} must be greater than zero.");
        }
    }
}
