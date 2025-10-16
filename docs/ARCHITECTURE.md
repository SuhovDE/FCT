# AeroNexus Forecast Studio – Foundation

This document captures the initial architectural decisions implemented during the
foundation phase of the AeroNexus Forecast Studio program.

## Solution layout

- `AeroNexus.ForecastStudio.Server` – Blazor Server front end hosting shared UI shell
  and API endpoints.
- `AeroNexus.ForecastStudio.Infrastructure` – Entity Framework Core data access layer
  with SQL Server provider and type configurations.
- `AeroNexus.ForecastStudio.Domain` – Core domain models representing airports,
  airlines, and planning scenarios.

## Next steps

- Introduce authentication/authorization infrastructure aligned with stakeholder roles.
- Implement import pipelines using background jobs and resilient storage.
- Expand the domain model to cover flights, load products, and forecast adjustments.
