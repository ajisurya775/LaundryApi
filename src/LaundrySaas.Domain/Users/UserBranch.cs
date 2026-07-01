using LaundrySaas.SharedKernel.Interfaces;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Users;

public class UserBranch : Entity, IMustHaveTenant
{
    public Guid UserId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid TenantId { get; private set; }

    public UserBranch(Guid userId, Guid branchId, Guid tenantId) : base()
    {
        UserId = userId;
        BranchId = branchId;
        TenantId = tenantId;
    }

    // EF Core Constructor
    private UserBranch()
    {
    }
}
