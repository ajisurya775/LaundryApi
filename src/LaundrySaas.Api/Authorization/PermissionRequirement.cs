using Microsoft.AspNetCore.Authorization;

namespace LaundrySaas.Api.Authorization;

/// <summary>
/// Authorization requirement representing a specific permission key.
/// Used with PermissionPolicyProvider to create dynamic policies.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionKey { get; }

    public PermissionRequirement(string permissionKey)
    {
        PermissionKey = permissionKey;
    }
}
