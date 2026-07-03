# ADR 0003: Explicit Decimal Scale Mappings

## Status
Approved

## Context
Standard global decimal mappings in EF Core can inadvertently round off or clip values requiring precise fractional scales (such as quantities, weight, tax rates, or package sizes), while money amounts generally require a fixed 2 decimal places.

## Decision
1. We introduce a `Money` Value Object (owning a `Currency` Value Object) for all monetary attributes.
2. We map money attributes explicitly in `IEntityTypeConfiguration` implementations with `.HasPrecision(18, 2)` and separate columns for amount and currency code.
3. We avoid any global decimal configurations in `OnModelCreating`, specifying precise scales explicitly per entity context (e.g. `TaxRate` has precision `5,2`, `Quantity` has precision `18,3`).

## Consequences
- Clean mathematical rules inside `Money` prevent precision loss or rounding mismatch errors.
- Database schemas explicitly match the domain value object scaling rules.
