namespace AeroNexus.ForecastStudio.Domain.Entities;

/// <summary>
/// Represents a reusable collection of filtering criteria that can be
/// applied across imports, forecasting, and analytics modules.
/// </summary>
public class GroupDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
        = string.Empty;

    public Guid ScenarioId { get; set; }
    public Scenario Scenario { get; set; } = default!;

    public bool IsShared { get; set; }
        = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<GroupCondition> Conditions { get; set; }
        = new List<GroupCondition>();
}

/// <summary>
/// Defines a single condition within a group definition.
/// </summary>
public class GroupCondition
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GroupDefinitionId { get; set; }
    public GroupDefinition GroupDefinition { get; set; } = default!;

    /// <summary>
    /// Indicates the field the condition targets (e.g., Airline.Code).
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Operator like Equals, Contains, In, GreaterThan.
    /// </summary>
    public string Operator { get; set; } = string.Empty;

    /// <summary>
    /// Value(s) expressed as JSON for multi-value operators.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    public int Order { get; set; }
        = 0;
}
