using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.SharedKernel.ValueObjects;

public class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email()
    {
        Value = string.Empty;
    }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Email cannot be empty.");
        }

        var trimmed = value.Trim();
        if (!EmailRegex.IsMatch(trimmed))
        {
            throw new DomainException($"Invalid email format: '{value}'");
        }

        Value = trimmed.ToLowerInvariant();
    }

    public static Email From(string value) => new(value);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
