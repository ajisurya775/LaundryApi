using System;

namespace LaundrySaas.Application.Identity.Services;

public interface ITokenService
{
    string GenerateToken(Guid userId, Guid tenantId, string email, string fullName);
}
