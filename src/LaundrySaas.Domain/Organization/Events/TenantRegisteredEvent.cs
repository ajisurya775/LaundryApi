using System;
using LaundrySaas.SharedKernel.Interfaces;

namespace LaundrySaas.Domain.Organization.Events;

public record TenantRegisteredEvent(
    Guid TenantId,
    string TenantName,
    string CompanyName,
    string CountryCode,
    string PhoneNumber,
    Guid OwnerId,
    string OwnerEmail,
    string OwnerFullName,
    string PasswordHash,
    Guid DefaultBranchId,
    Guid DefaultPosId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
