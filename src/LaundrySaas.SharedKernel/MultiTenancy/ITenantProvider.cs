using System;

namespace LaundrySaas.SharedKernel.MultiTenancy;

public interface ITenantProvider
{
    Guid? TenantId { get; }
}
