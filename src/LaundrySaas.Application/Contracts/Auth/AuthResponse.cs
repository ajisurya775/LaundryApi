using System;

namespace LaundrySaas.Application.Contracts.Auth;

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    Guid TenantId,
    string TenantName
);
