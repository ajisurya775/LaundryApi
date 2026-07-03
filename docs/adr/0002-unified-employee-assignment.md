# ADR 0002: Unified Employee Assignment

## Status
Approved

## Context
In the previous architecture, user relationships to branches and POS machines were modeled in separate tables (`UserBranch`, `UserPos`). This structure led to complex joins, duplication of roles/priority data, and difficulty managing overlapping temporal access bounds for multi-outlet laundry operators.

## Decision
We consolidate employee-to-branch and employee-to-POS mappings into a single unified table named `EmployeeAssignment`.
- Fields include `UserId`, `BranchId`, `RoleId`, `PosId`, `EmploymentType`, `Status`, `EffectiveFrom`, `EffectiveUntil`, `IsDefault`, and `Priority`.
- Higher priority values resolve overlaps when an employee holds multiple active assignments.

## Consequences
- Single table simplifies checking user permissions and machine/outlet scopes in authorization handlers.
- Reduced table count and index complexity in Postgres.
