namespace AeroNexus.ForecastStudio.Domain.Entities;

/// <summary>
/// Stores historical statistical data imported for comparison periods.
/// </summary>
public class AirportStatistic
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AirportId { get; set; }
    public Airport Airport { get; set; } = default!;

    public Guid ScenarioId { get; set; }
    public Scenario Scenario { get; set; } = default!;

    public DateOnly PeriodStart { get; set; }
        = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public DateOnly PeriodEnd { get; set; }
        = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public int TotalMovements { get; set; }
        = 0;

    public int TotalPassengers { get; set; }
        = 0;

    public int TotalFreightTonnes { get; set; }
        = 0;

    public string? SourceFileName { get; set; }
        = string.Empty;

    public DateTime ImportedAtUtc { get; set; } = DateTime.UtcNow;
}
