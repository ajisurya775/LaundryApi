using System;
using System.Threading;
using System.Threading.Tasks;
using LaundrySaas.Application.Common;
using LaundrySaas.Application.Contracts.Auth;
using LaundrySaas.Application.Identity.Services;
using LaundrySaas.Domain.Identity;
using LaundrySaas.Domain.Organization;
using LaundrySaas.SharedKernel.Interfaces;

namespace LaundrySaas.Application.Identity.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, ApiResponse<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ITokenService _tokenService;
    private readonly IAuthQueryService _authQueryService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        ITokenService tokenService,
        IAuthQueryService authQueryService)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _tokenService = tokenService;
        _authQueryService = authQueryService;
    }

    public async Task<ApiResponse<AuthResponse>> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
        {
            return ApiResponse<AuthResponse>.CreateError("Email and password are required.", "MissingCredentials");
        }

        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (user == null || !user.IsActive)
        {
            return ApiResponse<AuthResponse>.CreateError("Authentication failed.", "Invalid email or password.");
        }

        if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
        {
            return ApiResponse<AuthResponse>.CreateError("Authentication failed.", "Invalid email or password.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(user.TenantId, cancellationToken);
        if (tenant == null || !tenant.IsActive)
        {
            return ApiResponse<AuthResponse>.CreateError("Authentication failed.", "Associated tenant is inactive or not found.");
        }

        var token = _tokenService.GenerateToken(user.Id, tenant.Id, user.Email, user.FullName);
        var expiry = DateTime.UtcNow.AddHours(24);

        var authResponse = await _authQueryService.BuildAuthResponseAsync(token, expiry, user.Id, tenant.Id, cancellationToken);

        return ApiResponse<AuthResponse>.CreateSuccess(authResponse, "Login successful.");
    }
}
