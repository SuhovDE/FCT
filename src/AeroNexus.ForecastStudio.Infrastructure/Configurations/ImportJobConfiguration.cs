using AeroNexus.ForecastStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroNexus.ForecastStudio.Infrastructure.Configurations;

public class ImportJobConfiguration : IEntityTypeConfiguration<ImportJob>
{
    public void Configure(EntityTypeBuilder<ImportJob> builder)
    {
        builder.ToTable("ImportJobs");

        builder.HasKey(job => job.Id);

        builder.Property(job => job.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(job => job.SourceFileName)
            .HasMaxLength(500);

        builder.Property(job => job.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(job => job.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasMany(job => job.Messages)
            .WithOne(message => message.ImportJob)
            .HasForeignKey(message => message.ImportJobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(job => job.ColumnMappings)
            .WithOne(mapping => mapping.ImportJob)
            .HasForeignKey(mapping => mapping.ImportJobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(job => job.FlightCandidates)
            .WithOne(candidate => candidate.ImportJob)
            .HasForeignKey(candidate => candidate.ImportJobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ImportJobMessageConfiguration : IEntityTypeConfiguration<ImportJobMessage>
{
    public void Configure(EntityTypeBuilder<ImportJobMessage> builder)
    {
        builder.ToTable("ImportJobMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Level)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(message => message.Message)
            .IsRequired()
            .HasMaxLength(2000);
    }
}

public class ImportColumnMappingConfiguration : IEntityTypeConfiguration<ImportColumnMapping>
{
    public void Configure(EntityTypeBuilder<ImportColumnMapping> builder)
    {
        builder.ToTable("ImportColumnMappings");

        builder.HasKey(mapping => mapping.Id);

        builder.Property(mapping => mapping.SourceColumn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(mapping => mapping.TargetField)
            .IsRequired()
            .HasMaxLength(200);
    }
}

public class ImportFlightCandidateConfiguration : IEntityTypeConfiguration<ImportFlightCandidate>
{
    public void Configure(EntityTypeBuilder<ImportFlightCandidate> builder)
    {
        builder.ToTable("ImportFlightCandidates");

        builder.HasKey(candidate => candidate.Id);

        builder.Property(candidate => candidate.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(candidate => candidate.FlightNumber)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(candidate => candidate.OriginAirportCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(candidate => candidate.DestinationAirportCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(candidate => candidate.Details)
            .HasMaxLength(2000);
    }
}
