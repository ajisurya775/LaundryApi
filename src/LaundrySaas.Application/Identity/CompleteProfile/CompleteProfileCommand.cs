using System;

namespace LaundrySaas.Application.Identity.CompleteProfile;

public record CompleteProfileCommand(
    Guid UserId,
    string Password,
    string CountryCode,
    string PhoneNumber);
