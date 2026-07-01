using LaundrySaas.SharedKernel.Interfaces;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Users;

public class RoleMenuAccess : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public UserRole Role { get; private set; }
    public Guid MenuItemId { get; private set; }

    public RoleMenuAccess(Guid id, Guid tenantId, UserRole role, Guid menuItemId) : base(id)
    {
        TenantId = tenantId;
        Role = role;
        MenuItemId = menuItemId;
    }

    // EF Core Constructor
    private RoleMenuAccess()
    {
    }
}
