namespace LaundrySaas.Application.Contracts.Auth;

public record RegisterTenantRequest(
    string CompanyName,
    string TenantName,
    string OwnerFullName,
    string OwnerEmail,
    string Password,
    string CountryCode,
    string PhoneNumber
);
