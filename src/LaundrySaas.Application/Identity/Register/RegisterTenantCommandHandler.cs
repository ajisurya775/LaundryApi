using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LaundrySaas.Application.Common;
using LaundrySaas.Application.Contracts.Auth;
using LaundrySaas.Application.Identity.Services;
using LaundrySaas.Domain.Identity;
using LaundrySaas.Domain.Organization;
using LaundrySaas.Domain.Organization.Events;
using LaundrySaas.Domain.POS;
using LaundrySaas.Domain.Billing;
using LaundrySaas.SharedKernel.Interfaces;

namespace LaundrySaas.Application.Identity.Register;

public class RegisterTenantCommandHandler : ICommandHandler<RegisterTenantCommand, ApiResponse<AuthResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IPosRepository _posRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IEmployeeAssignmentRepository _employeeAssignmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public RegisterTenantCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IBranchRepository branchRepository,
        IPosRepository posRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IEmployeeAssignmentRepository employeeAssignmentRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IDomainEventDispatcher domainEventDispatcher)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _posRepository = posRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _employeeAssignmentRepository = employeeAssignmentRepository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<ApiResponse<AuthResponse>> HandleAsync(RegisterTenantCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.CompanyName) ||
            string.IsNullOrWhiteSpace(command.TenantName) ||
            string.IsNullOrWhiteSpace(command.OwnerFullName) ||
            string.IsNullOrWhiteSpace(command.OwnerEmail) ||
            string.IsNullOrWhiteSpace(command.Password) ||
            string.IsNullOrWhiteSpace(command.CountryCode) ||
            string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            return ApiResponse<AuthResponse>.CreateError("All fields are required.", "Missing fields.");
        }

        var tenantExists = await _tenantRepository.ExistsByNameAsync(command.TenantName, cancellationToken);
        if (tenantExists)
        {
            return ApiResponse<AuthResponse>.CreateError("Tenant name is already taken.", "TenantNameTaken");
        }

        var emailExists = await _userRepository.GetByEmailAsync(command.OwnerEmail, cancellationToken);
        if (emailExists != null)
        {
            return ApiResponse<AuthResponse>.CreateError("Email address is already registered.", "EmailRegistered");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Create Tenant
            var tenantId = Guid.NewGuid();
            var tenant = new Tenant(tenantId, command.TenantName, command.CompanyName, command.CountryCode, command.PhoneNumber);
            await _tenantRepository.AddAsync(tenant, cancellationToken);

            // 2. Create Owner User
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);
            var userId = Guid.NewGuid();
            var user = new User(userId, tenantId, command.OwnerFullName, command.OwnerEmail, passwordHash);
            await _userRepository.AddAsync(user, cancellationToken);

            // 3. Dispatch TenantRegisteredEvent to initialize other modules
            var branchId = Guid.NewGuid();
            var posId = Guid.NewGuid();

            var registerEvent = new TenantRegisteredEvent(
                tenantId,
                command.TenantName,
                command.CompanyName,
                command.CountryCode,
                command.PhoneNumber,
                userId,
                command.OwnerEmail,
                command.OwnerFullName,
                passwordHash,
                branchId,
                posId);

            // Handlers will execute in the same transaction
            await _domainEventDispatcher.DispatchAsync(registerEvent, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // 4. Generate token & response
            var token = _tokenService.GenerateToken(user.Id, tenant.Id, user.Email, user.FullName);
            var expiry = DateTime.UtcNow.AddHours(24);

            var permissions = await _permissionRepository.GetAllAsync(cancellationToken);
            var ownerPermissions = permissions.Select(p => p.Key).ToList();

            var response = new AuthResponse(
                token,
                expiry,
                new UserDto(user.Id, user.Email, user.FullName, tenant.Id, tenant.Name),
                new List<UserBranchDto> { new(branchId, "Central Branch", "Owner", "OWNER") },
                ownerPermissions,
                new List<BranchDto> { new(branchId, "Central Branch", "Default Address") },
                new List<PosDto> { new(posId, "Kasir 1", branchId, "Central Branch") },
                new PosDto(posId, "Kasir 1", branchId, "Central Branch")
            );

            return ApiResponse<AuthResponse>.CreateSuccess(response, "Tenant registered successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return ApiResponse<AuthResponse>.CreateError("Registration failed.", ex.Message);
        }
    }
}
