# AeroNexus Forecast Studio – Foundation

This document captures the initial architectural decisions implemented during the
foundation phase of the AeroNexus Forecast Studio program.

## Solution layout

- `AeroNexus.ForecastStudio.Server` – Blazor Server front end hosting shared UI shell,
  group editor, and import wizards.
- `AeroNexus.ForecastStudio.Infrastructure` – Entity Framework Core data access layer
  with SQL Server provider, entity configurations, and service implementations for
  imports and groups.
- `AeroNexus.ForecastStudio.Domain` – Core domain models representing airports,
  airlines, scenarios, flights, statistical datasets, and import job metadata.

## Implemented capabilities

- **Group management** – CRUD operations for scenario-bound filter definitions with
  ordered condition lists.
- **Import orchestration** – Services to validate, analyse, preview, and commit
  statistical datasets and flight schedules, including automatic creation of
  placeholder reference data.
- **Database schema evolution** – Configurations for flights, imports, and
  statistics that enable future migrations and seeding.

## Next steps

- Introduce authentication/authorization aligned with stakeholder roles.
- Extend forecasting and load management services, including adjustment and FEG
  engines.
- Offload long-running imports to background jobs with resumable progress tracking.
