namespace AeroNexus.ForecastStudio.Domain.Entities;

/// <summary>
/// Represents an airline carrier that owns scheduled flights in scenarios.
/// </summary>
public class Airline
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Alliance { get; set; }
        = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
