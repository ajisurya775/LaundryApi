using LaundrySaas.SharedKernel.Interfaces;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Users;

public class User : Entity, IMustHaveTenant
{
    private readonly List<UserBranch> _assignedBranches = new();

    public Guid TenantId { get; private set; }
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string? PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    // Firebase / OAuth fields
    public string? FirebaseUid { get; private set; }
    public AuthProvider AuthProvider { get; private set; }
    public string? PhotoUrl { get; private set; }
    public bool EmailVerified { get; private set; }

    /// <summary>
    /// Profile is complete when the user has set a password.
    /// Google login users must complete their profile before using the platform.
    /// </summary>
    public bool IsProfileComplete => !string.IsNullOrEmpty(PasswordHash);

    public IReadOnlyCollection<UserBranch> AssignedBranches => _assignedBranches.AsReadOnly();

    /// <summary>
    /// Constructor for traditional email/password registration.
    /// </summary>
    public User(Guid id, Guid tenantId, string fullName, string email, string passwordHash, UserRole role) : base(id)
    {
        TenantId = tenantId;
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        AuthProvider = AuthProvider.EmailPassword;
        EmailVerified = false;
        IsActive = true;
    }

    /// <summary>
    /// Constructor for Google (Firebase) login.
    /// PasswordHash is null — user must complete profile by setting a password.
    /// </summary>
    public User(Guid id, Guid tenantId, string fullName, string email, string firebaseUid, string? photoUrl, UserRole role) : base(id)
    {
        TenantId = tenantId;
        FullName = fullName;
        Email = email;
        PasswordHash = null;
        FirebaseUid = firebaseUid;
        PhotoUrl = photoUrl;
        Role = role;
        AuthProvider = AuthProvider.Google;
        EmailVerified = true; // Google emails are always verified
        IsActive = true;
    }

    // EF Core Constructor
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

    /// <summary>
    /// Sets or updates the password. Required for Google login users to complete their profile.
    /// </summary>
    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        PasswordHash = passwordHash;
    }

    /// <summary>
    /// Links a Firebase account to an existing user.
    /// </summary>
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

    public void UpdateRole(UserRole newRole)
    {
        Role = newRole;
    }

    public void AssignToBranch(Guid branchId)
    {
        // Owner doesn't need explicit branch assignments since they have access to all branches
        if (Role == UserRole.Owner)
        {
            return;
        }

        if (!_assignedBranches.Any(ub => ub.BranchId == branchId))
        {
            _assignedBranches.Add(new UserBranch(Id, branchId, TenantId));
        }
    }

    public void RemoveFromBranch(Guid branchId)
    {
        var existing = _assignedBranches.FirstOrDefault(ub => ub.BranchId == branchId);
        if (existing is not null)
        {
            _assignedBranches.Remove(existing);
        }
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
