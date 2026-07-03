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

public class LoginFirebaseCommandHandler : ICommandHandler<LoginFirebaseCommand, ApiResponse<object>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IFirebaseAuthService _firebaseAuthService;
    private readonly ITokenService _tokenService;
    private readonly IAuthQueryService _authQueryService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginFirebaseCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IFirebaseAuthService firebaseAuthService,
        ITokenService tokenService,
        IAuthQueryService authQueryService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _firebaseAuthService = firebaseAuthService;
        _tokenService = tokenService;
        _authQueryService = authQueryService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<object>> HandleAsync(LoginFirebaseCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.IdToken))
        {
            return ApiResponse<object>.CreateError("Firebase ID Token is required.", "MissingToken");
        }

        FirebaseTokenPayload payload;
        try
        {
            payload = await _firebaseAuthService.VerifyIdTokenAsync(command.IdToken);
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.CreateError("Firebase token verification failed.", ex.Message);
        }

        var user = await _userRepository.GetByFirebaseUidAsync(payload.Uid, cancellationToken);
        if (user == null)
        {
            user = await _userRepository.GetByEmailAsync(payload.Email, cancellationToken);
        }

        if (user == null)
        {
            return ApiResponse<object>.CreateError("User not registered. Please register a tenant first.", "UserNotRegistered");
        }

        if (!user.IsActive)
        {
            return ApiResponse<object>.CreateError("Authentication failed. User is inactive.", "UserInactive");
        }

        bool profileUpdated = false;
        if (string.IsNullOrEmpty(user.FirebaseUid))
        {
            user.LinkFirebaseAccount(payload.Uid, payload.PictureUrl);
            profileUpdated = true;
        }
        else if (user.PhotoUrl != payload.PictureUrl)
        {
            user.UpdatePhotoUrl(payload.PictureUrl);
            profileUpdated = true;
        }

        if (profileUpdated)
        {
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var tenant = await _tenantRepository.GetByIdAsync(user.TenantId, cancellationToken);
        if (tenant == null || !tenant.IsActive)
        {
            return ApiResponse<object>.CreateError("Authentication failed. Associated tenant is inactive or not found.", "TenantInactive");
        }

        if (string.IsNullOrEmpty(tenant.CountryCode) || string.IsNullOrEmpty(tenant.PhoneNumber))
        {
            return ApiResponse<object>.CreateSuccess(new
            {
                tenantId = tenant.Id,
                userId = user.Id,
                email = user.Email,
                isProfileComplete = false
            }, "Tenant profile is incomplete. Please complete your profile.");
        }

        var token = _tokenService.GenerateToken(user.Id, tenant.Id, user.Email, user.FullName);
        var expiry = DateTime.UtcNow.AddHours(24);

        var authResponse = await _authQueryService.BuildAuthResponseAsync(token, expiry, user.Id, tenant.Id, cancellationToken);

        return ApiResponse<object>.CreateSuccess(authResponse, "Login successful.");
    }
}
