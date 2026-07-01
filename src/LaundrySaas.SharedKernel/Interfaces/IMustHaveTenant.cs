namespace LaundrySaas.SharedKernel.Interfaces;

public interface IMustHaveTenant
{
    Guid TenantId { get; }
}
