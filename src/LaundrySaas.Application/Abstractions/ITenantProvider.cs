namespace LaundrySaas.Application.Abstractions;

public interface ITenantProvider
{
    Guid? TenantId { get; }
}
