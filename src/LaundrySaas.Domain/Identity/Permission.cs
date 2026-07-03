using System;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Identity;

public class Permission : Entity
{
    public string Key { get; private set; }
    public string DisplayName { get; private set; }
    public string? Description { get; private set; }
    public string Category { get; private set; }
    public bool IsActive { get; private set; }

    public Permission(Guid id, string key, string displayName, string category, string? description = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Permission key is required.", nameof(key));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));

        Key = key.Trim();
        DisplayName = displayName;
        Category = category;
        Description = description;
        IsActive = true;
    }

    private Permission()
    {
        Key = null!;
        DisplayName = null!;
        Category = null!;
    }

    public void UpdateDetails(string displayName, string category, string? description)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));

        DisplayName = displayName;
        Category = category;
        Description = description;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
