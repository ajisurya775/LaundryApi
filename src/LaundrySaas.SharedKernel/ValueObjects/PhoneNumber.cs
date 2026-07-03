using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.SharedKernel.ValueObjects;

public class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$", // Simple E.164 phone number format validation
        RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber()
    {
        Value = string.Empty;
    }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Phone number cannot be empty.");
        }

        // Clean formatting characters, but keep leading + if present
        var cleaned = Regex.Replace(value.Trim(), @"[\s\-\(\)]", "");

        if (!PhoneRegex.IsMatch(cleaned))
        {
            throw new DomainException($"Invalid E.164 phone number: '{value}'");
        }

        Value = cleaned;
    }

    public static PhoneNumber From(string value) => new(value);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
