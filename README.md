# AeroNexus Forecast Studio

AeroNexus Forecast Studio is the modern replacement initiative for the Beontra
BTactical product. This repository currently hosts the foundation phase artifacts:

- A .NET 8 Blazor Server host project with Bootstrap-powered layout.
- Entity Framework Core infrastructure prepared for SQL Server.
- Domain models capturing the first slice of reference data and planning scenarios.

## Getting started

1. Install the .NET 8 SDK and SQL Server (or Azure SQL) locally.
2. Update `src/AeroNexus.ForecastStudio.Server/appsettings.json` with your preferred
   database connection string.
3. Run `dotnet ef database update` (migrations coming soon) to create the schema.
4. Launch the application with `dotnet run --project src/AeroNexus.ForecastStudio.Server`.

## Next steps

The upcoming iterations will focus on:

- Automated import workflows for statistical and schedule data.
- Scenario editing tools for groups, constraints, and rotations.
- Forecasting engines, analytics, and publishing pipelines.
