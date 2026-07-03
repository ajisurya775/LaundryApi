using System;
using Microsoft.AspNetCore.Authorization;

namespace LaundrySaas.Api.Authorization;

/// <summary>
/// Attribute to require a specific permission for an endpoint.
/// Usage: [RequirePermission("Order.Create")]
/// Maps to a dynamic authorization policy via PermissionPolicyProvider.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permissionKey)
        : base(policy: $"{PermissionPolicyProvider.PolicyPrefix}{permissionKey}")
    {
    }
}
