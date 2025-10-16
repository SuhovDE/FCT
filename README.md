# AeroNexus Forecast Studio

AeroNexus Forecast Studio is the modern replacement initiative for the Beontra
BTactical product. This repository now includes the first functional slices of
the imports and group management experiences:

- A .NET 8 Blazor Server host project with Bootstrap-powered layout and navigation.
- Entity Framework Core infrastructure prepared for SQL Server with flight,
  statistical, and import-tracking entities.
- A group editor surface for creating, updating, and deleting reusable filter
  definitions per scenario.
- End-to-end statistical and schedule import wizards covering validation,
  column mapping, preview, and commit workflows.

## Getting started

1. Install the .NET 8 SDK and SQL Server (or Azure SQL) locally.
2. Update `src/AeroNexus.ForecastStudio.Server/appsettings.json` with your preferred
   database connection string.
3. Run `dotnet ef database update` (migrations coming soon) to create the schema.
4. Launch the application with `dotnet run --project src/AeroNexus.ForecastStudio.Server`.

## Key features available

- **Group management** – Navigate to <code>/groups</code> to manage shared and
  scenario-specific filter groups with ordered condition lists.
- **Statistical import wizard** – Upload airport statistics (.csv or .zip),
  validate structure, confirm column mapping, and persist comparison baselines.
- **Schedule import wizard** – Step through file validation, mapping, filter and
  business rule settings, preview differences, and commit flights with automatic
  placeholder reference data creation.

## Next steps

- Expand forecasting workbenches (load factor view, forecast adjustments, flight
  event generator).
- Introduce authentication/authorization aligned with airport planning roles.
- Implement background processing for long-running imports and scenario
  publications.
