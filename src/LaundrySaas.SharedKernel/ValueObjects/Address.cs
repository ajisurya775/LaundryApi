using System;
using System.Collections.Generic;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.SharedKernel.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string? State { get; }
    public string? PostalCode { get; }
    public string Country { get; }

    private Address()
    {
        Street = string.Empty;
        City = string.Empty;
        Country = string.Empty;
    }

    public Address(string street, string city, string? state, string? postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street)) throw new DomainException("Street is required.");
        if (string.IsNullOrWhiteSpace(city)) throw new DomainException("City is required.");
        if (string.IsNullOrWhiteSpace(country)) throw new DomainException("Country is required.");

        Street = street.Trim();
        City = city.Trim();
        State = state?.Trim();
        PostalCode = postalCode?.Trim();
        Country = country.Trim();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Street;
        yield return City;
        yield return State ?? string.Empty;
        yield return PostalCode ?? string.Empty;
        yield return Country;
    }

    public override string ToString() => 
        $"{Street}, {City}{(string.IsNullOrWhiteSpace(State) ? "" : ", " + State)}{(string.IsNullOrWhiteSpace(PostalCode) ? "" : " " + PostalCode)}, {Country}";
}
