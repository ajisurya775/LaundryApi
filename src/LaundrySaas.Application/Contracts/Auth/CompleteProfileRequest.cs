using System;

namespace LaundrySaas.Application.Contracts.Auth;

public record CompleteProfileRequest(
    Guid UserId,
    string Password,
    string CountryCode,
    string PhoneNumber
);
