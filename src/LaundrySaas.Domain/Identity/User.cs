using System;
using LaundrySaas.Domain.Identity.Enums;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Identity;

public class User : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string? PasswordHash { get; private set; }
    public bool IsActive { get; private set; }

    // Firebase / OAuth fields
    public string? FirebaseUid { get; private set; }
    public AuthProvider AuthProvider { get; private set; }
    public string? PhotoUrl { get; private set; }
    public bool EmailVerified { get; private set; }

    public bool IsProfileComplete => !string.IsNullOrEmpty(PasswordHash);

    public User(Guid id, Guid tenantId, string fullName, string email, string passwordHash) : base(id)
    {
        TenantId = tenantId;
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        AuthProvider = AuthProvider.EmailPassword;
        EmailVerified = false;
        IsActive = true;
    }

    public User(Guid id, Guid tenantId, string fullName, string email, string firebaseUid, string? photoUrl) : base(id)
    {
        TenantId = tenantId;
        FullName = fullName;
        Email = email;
        PasswordHash = null;
        FirebaseUid = firebaseUid;
        PhotoUrl = photoUrl;
        AuthProvider = AuthProvider.Google;
        EmailVerified = true;
        IsActive = true;
    }

    private User()
    {
        FullName = null!;
        Email = null!;
    }

    public void UpdateProfile(string fullName, string email)
    {
        FullName = fullName;
        Email = email;
    }

    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        PasswordHash = passwordHash;
    }

    public void LinkFirebaseAccount(string firebaseUid, string? photoUrl)
    {
        if (string.IsNullOrWhiteSpace(firebaseUid))
            throw new ArgumentException("Firebase UID cannot be empty.", nameof(firebaseUid));

        FirebaseUid = firebaseUid;
        PhotoUrl = photoUrl;
    }

    public void UpdatePhotoUrl(string? photoUrl)
    {
        PhotoUrl = photoUrl;
    }

    public void MarkEmailVerified()
    {
        EmailVerified = true;
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
