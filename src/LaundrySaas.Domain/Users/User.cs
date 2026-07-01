using LaundrySaas.SharedKernel.Interfaces;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Users;

public class User : Entity, IMustHaveTenant
{
    private readonly List<UserBranch> _assignedBranches = new();

    public Guid TenantId { get; private set; }
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<UserBranch> AssignedBranches => _assignedBranches.AsReadOnly();

    public User(Guid id, Guid tenantId, string fullName, string email, string passwordHash, UserRole role) : base(id)
    {
        TenantId = tenantId;
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    // EF Core Constructor
    private User()
    {
        FullName = null!;
        Email = null!;
        PasswordHash = null!;
    }

    public void UpdateProfile(string fullName, string email)
    {
        FullName = fullName;
        Email = email;
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
