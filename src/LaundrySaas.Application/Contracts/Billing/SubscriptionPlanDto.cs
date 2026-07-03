using System;

namespace LaundrySaas.Application.Contracts.Billing;

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    decimal PriceAmount,
    string PriceCurrency,
    bool HasPos,
    bool HasInventory,
    bool HasAccounting,
    decimal ExtraCredit,
    bool IsActive
);
