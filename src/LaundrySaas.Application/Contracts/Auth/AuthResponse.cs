using System;
using System.Collections.Generic;

namespace LaundrySaas.Application.Contracts.Auth;

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    UserDto User,
    List<UserBranchDto> Roles,
    List<string> Permissions,
    List<BranchDto> Branches,
    List<PosDto> Poses,
    PosDto? DefaultPos
);

public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    Guid TenantId,
    string TenantName
);

public record BranchDto(
    Guid Id,
    string Name,
    string Address
);

public record PosDto(
    Guid Id,
    string Name,
    Guid BranchId,
    string BranchName
);

public record UserBranchDto(
    Guid? BranchId,
    string? BranchName,
    string RoleName,
    string RoleCode
);
