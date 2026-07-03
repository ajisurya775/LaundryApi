using System;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Infrastructure.Persistence.Audit;

/// <summary>
/// Infrastructure audit log entity to record state changes, cashier checkouts, refunds, and critical compliance actions.
/// </summary>
public class AuditLog : Entity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Action { get; private set; }
    public string EntityName { get; private set; }
    public string EntityId { get; private set; }
    public string? Before { get; private set; }
    public string? After { get; private set; }
    public string? IPAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public AuditLog(
        Guid id,
        Guid tenantId,
        Guid? userId,
        string action,
        string entityName,
        string entityId,
        string? before,
        string? after,
        string? ipAddress,
        string? userAgent) : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        Before = before;
        After = after;
        IPAddress = ipAddress;
        UserAgent = userAgent;
    }

    // EF Core Constructor
    private AuditLog()
    {
        Action = null!;
        EntityName = null!;
        EntityId = null!;
    }
}
