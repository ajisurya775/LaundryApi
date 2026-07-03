# ADR 0001: Modular Monolith Refactoring

## Status
Approved

## Context
The LaundrySaas codebase was structured as a traditional layered architecture, causing logical dependencies and making it difficult to split the application into separate services in the future. Business boundaries were blurred, with cross-module entity relationships loaded via direct navigational properties.

## Decision
We refactor the codebase into a true Modular Monolith.
1. Code is organized by **Business Capability** (Bounded Context) under `Identity`, `Organization`, `POS`, `Sales`, `Catalog`, `Customer`, `Inventory`, and `Billing` modules, rather than technical layers.
2. Direct EF Core navigational properties across context boundaries are replaced by **ID-based references**.
3. Cross-module orchestration and domain initializations are handled via **Domain Events** (e.g. `TenantRegisteredEvent`) dispatched within a single database transaction.

## Consequences
- Strict boundaries prevent accidental coupling between business areas.
- Independent modules can be easily extracted into microservices in the future.
- Queries spanning multiple modules must use query services or integration logic instead of direct navigational queries.
