using AeroNexus.ForecastStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroNexus.ForecastStudio.Infrastructure.Configurations;

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        builder.ToTable("Flights");

        builder.HasKey(flight => flight.Id);

        builder.Property(flight => flight.FlightNumber)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(flight => flight.OriginAirportCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(flight => flight.DestinationAirportCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(flight => flight.AircraftType)
            .HasMaxLength(10);

        builder.HasOne(flight => flight.Scenario)
            .WithMany()
            .HasForeignKey(flight => flight.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(flight => flight.Airline)
            .WithMany()
            .HasForeignKey(flight => flight.AirlineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
