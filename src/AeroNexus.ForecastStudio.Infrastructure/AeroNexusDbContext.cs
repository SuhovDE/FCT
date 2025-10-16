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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AeroNexusDbContext).Assembly);
    }
}
