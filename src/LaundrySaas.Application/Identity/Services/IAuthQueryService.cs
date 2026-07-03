using System;
using System.Threading;
using System.Threading.Tasks;
using LaundrySaas.Application.Contracts.Auth;

namespace LaundrySaas.Application.Identity.Services;

public interface IAuthQueryService
{
    Task<AuthResponse> BuildAuthResponseAsync(string token, DateTime expiry, Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}
