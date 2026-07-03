using System;
using System.Threading;
using System.Threading.Tasks;
using LaundrySaas.Application.Common;
using LaundrySaas.Application.Contracts.Auth;
using LaundrySaas.Application.Identity.Services;
using LaundrySaas.Domain.Identity;
using LaundrySaas.Domain.Organization;
using LaundrySaas.SharedKernel.Interfaces;

namespace LaundrySaas.Application.Identity.CompleteProfile;

public class CompleteProfileCommandHandler : ICommandHandler<CompleteProfileCommand, ApiResponse<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ITokenService _tokenService;
    private readonly IAuthQueryService _authQueryService;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteProfileCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        ITokenService tokenService,
        IAuthQueryService authQueryService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _tokenService = tokenService;
        _authQueryService = authQueryService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<AuthResponse>> HandleAsync(CompleteProfileCommand command, CancellationToken cancellationToken = default)
    {
        if (command.UserId == Guid.Empty ||
            string.IsNullOrWhiteSpace(command.Password) ||
            string.IsNullOrWhiteSpace(command.CountryCode) ||
            string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            return ApiResponse<AuthResponse>.CreateError("UserId, Password, CountryCode, and PhoneNumber are required.", "MissingFields");
        }

        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user == null)
        {
            return ApiResponse<AuthResponse>.CreateError("User not found.", "InvalidUserId");
        }

        if (!user.IsActive)
        {
            return ApiResponse<AuthResponse>.CreateError("Authentication failed. User is inactive.", "UserInactive");
        }

        var tenant = await _tenantRepository.GetByIdAsync(user.TenantId, cancellationToken);
        if (tenant == null || !tenant.IsActive)
        {
            return ApiResponse<AuthResponse>.CreateError("Associated active tenant not found.", "TenantInactiveOrNotFound");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);
            user.SetPassword(passwordHash);
            _userRepository.Update(user);

            tenant.UpdateDetails(tenant.Name, tenant.CompanyName, command.CountryCode, command.PhoneNumber);
            _tenantRepository.Update(tenant);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var token = _tokenService.GenerateToken(user.Id, tenant.Id, user.Email, user.FullName);
            var expiry = DateTime.UtcNow.AddHours(24);

            var authResponse = await _authQueryService.BuildAuthResponseAsync(token, expiry, user.Id, tenant.Id, cancellationToken);

            return ApiResponse<AuthResponse>.CreateSuccess(authResponse, "Profile completed successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return ApiResponse<AuthResponse>.CreateError("Failed to complete profile.", ex.Message);
        }
    }
}
