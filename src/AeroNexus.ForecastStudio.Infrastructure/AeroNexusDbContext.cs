using AeroNexus.ForecastStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AeroNexus.ForecastStudio.Infrastructure;

/// <summary>
/// Primary EF Core database context for AeroNexus Forecast Studio.
/// </summary>
public class AeroNexusDbContext : DbContext
{
    public AeroNexusDbContext(DbContextOptions<AeroNexusDbContext> options)
        : base(options)
    {
    }

    public DbSet<Airport> Airports => Set<Airport>();
    public DbSet<Airline> Airlines => Set<Airline>();
    public DbSet<Scenario> Scenarios => Set<Scenario>();
    public DbSet<ScenarioAirport> ScenarioAirports => Set<ScenarioAirport>();
    public DbSet<GroupDefinition> Groups => Set<GroupDefinition>();
    public DbSet<GroupCondition> GroupConditions => Set<GroupCondition>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<ImportJobMessage> ImportJobMessages => Set<ImportJobMessage>();
    public DbSet<ImportColumnMapping> ImportColumnMappings => Set<ImportColumnMapping>();
    public DbSet<ImportFlightCandidate> ImportFlightCandidates => Set<ImportFlightCandidate>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<AirportStatistic> AirportStatistics => Set<AirportStatistic>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AeroNexusDbContext).Assembly);
    }
}
