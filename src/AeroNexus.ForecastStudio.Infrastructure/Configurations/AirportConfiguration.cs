using AeroNexus.ForecastStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroNexus.ForecastStudio.Infrastructure.Configurations;

public class AirportConfiguration : IEntityTypeConfiguration<Airport>
{
    public void Configure(EntityTypeBuilder<Airport> builder)
    {
        builder.ToTable("Airports");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.IataCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(a => a.IcaoCode)
            .HasMaxLength(4);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Country)
            .HasMaxLength(100);

        builder.Property(a => a.CreatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
