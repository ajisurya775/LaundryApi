namespace LaundrySaas.Domain.Billing.Exceptions;

public class InsufficientCreditException : Exception
{
    public Guid TenantId { get; }
    public decimal CurrentBalance { get; }
    public decimal RequiredAmount { get; }

    public InsufficientCreditException(Guid tenantId, decimal currentBalance, decimal requiredAmount)
        : base($"Insufficient credit for tenant '{tenantId}'. Current balance: {currentBalance:N0}, required: {requiredAmount:N0}.")
    {
        TenantId = tenantId;
        CurrentBalance = currentBalance;
        RequiredAmount = requiredAmount;
    }
}
