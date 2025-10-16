using AeroNexus.ForecastStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroNexus.ForecastStudio.Infrastructure.Configurations;

public class AirportStatisticConfiguration : IEntityTypeConfiguration<AirportStatistic>
{
    public void Configure(EntityTypeBuilder<AirportStatistic> builder)
    {
        builder.ToTable("AirportStatistics");

        builder.HasKey(statistic => statistic.Id);

        builder.HasOne(statistic => statistic.Airport)
            .WithMany()
            .HasForeignKey(statistic => statistic.AirportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(statistic => statistic.Scenario)
            .WithMany()
            .HasForeignKey(statistic => statistic.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(statistic => statistic.SourceFileName)
            .HasMaxLength(500);
    }
}
