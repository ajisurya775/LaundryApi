using System;

namespace LaundrySaas.SharedKernel.MultiTenancy;

public interface IMustHaveTenant
{
    Guid TenantId { get; }
}
