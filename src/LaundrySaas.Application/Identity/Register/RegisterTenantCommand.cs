namespace LaundrySaas.Application.Identity.Register;

public record RegisterTenantCommand(
    string TenantName,
    string CompanyName,
    string OwnerFullName,
    string OwnerEmail,
    string Password,
    string CountryCode,
    string PhoneNumber);
