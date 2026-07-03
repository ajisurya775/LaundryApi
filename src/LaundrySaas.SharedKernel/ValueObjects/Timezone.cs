using System;
using System.Collections.Generic;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.SharedKernel.ValueObjects;

public class Timezone : ValueObject
{
    public string Id { get; }

    private Timezone()
    {
        Id = "UTC";
    }

    public Timezone(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Timezone Id cannot be empty.");

        try
        {
            // Verify if it's a valid timezone
            TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new DomainException($"Timezone '{id}' was not found on this system.");
        }
        catch (InvalidTimeZoneException)
        {
            throw new DomainException($"Timezone '{id}' is invalid.");
        }

        Id = id;
    }

    public static Timezone UTC => new("UTC");

    public DateTime ConvertToLocal(DateTime utcDateTime)
    {
        var zoneInfo = TimeZoneInfo.FindSystemTimeZoneById(Id);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, zoneInfo);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Id;
    }

    public override string ToString() => Id;
}
