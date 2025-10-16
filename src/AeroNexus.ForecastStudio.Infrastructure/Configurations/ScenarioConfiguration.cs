using AeroNexus.ForecastStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroNexus.ForecastStudio.Infrastructure.Configurations;

public class ScenarioConfiguration : IEntityTypeConfiguration<Scenario>
{
    public void Configure(EntityTypeBuilder<Scenario> builder)
    {
        builder.ToTable("Scenarios");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.StartDate)
            .HasColumnType("date");

        builder.Property(s => s.EndDate)
            .HasColumnType("date");

        builder.HasMany(s => s.Airports)
            .WithOne(sa => sa.Scenario)
            .HasForeignKey(sa => sa.ScenarioId);
    }
}

public class ScenarioAirportConfiguration : IEntityTypeConfiguration<ScenarioAirport>
{
    public void Configure(EntityTypeBuilder<ScenarioAirport> builder)
    {
        builder.ToTable("ScenarioAirports");

        builder.HasKey(sa => new { sa.ScenarioId, sa.AirportId });

        builder.HasOne(sa => sa.Airport)
            .WithMany()
            .HasForeignKey(sa => sa.AirportId);
    }
}
