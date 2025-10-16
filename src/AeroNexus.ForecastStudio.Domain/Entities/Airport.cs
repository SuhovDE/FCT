namespace AeroNexus.ForecastStudio.Domain.Entities;

/// <summary>
/// Represents an airport used across schedule and forecast datasets.
/// </summary>
public class Airport
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string IataCode { get; set; } = string.Empty;

    public string? IcaoCode { get; set; }
        = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Country { get; set; }
        = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
