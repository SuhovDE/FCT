using AeroNexus.ForecastStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroNexus.ForecastStudio.Infrastructure.Configurations;

public class GroupDefinitionConfiguration : IEntityTypeConfiguration<GroupDefinition>
{
    public void Configure(EntityTypeBuilder<GroupDefinition> builder)
    {
        builder.ToTable("GroupDefinitions");

        builder.HasKey(group => group.Id);

        builder.Property(group => group.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(group => group.Description)
            .HasMaxLength(1000);

        builder.HasOne(group => group.Scenario)
            .WithMany()
            .HasForeignKey(group => group.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(group => group.Conditions)
            .WithOne(condition => condition.GroupDefinition)
            .HasForeignKey(condition => condition.GroupDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GroupConditionConfiguration : IEntityTypeConfiguration<GroupCondition>
{
    public void Configure(EntityTypeBuilder<GroupCondition> builder)
    {
        builder.ToTable("GroupConditions");

        builder.HasKey(condition => condition.Id);

        builder.Property(condition => condition.Field)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(condition => condition.Operator)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(condition => condition.Value)
            .IsRequired()
            .HasMaxLength(2000);
    }
}
