namespace AeroNexus.ForecastStudio.Domain.Entities;

/// <summary>
/// Represents a planning workspace with flights, loads, and forecasts.
/// </summary>
public class Scenario
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }
        = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public DateOnly EndDate { get; set; }
        = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public ICollection<ScenarioAirport> Airports { get; set; } = new List<ScenarioAirport>();
}

public class ScenarioAirport
{
    public Guid ScenarioId { get; set; }
    public Scenario Scenario { get; set; } = default!;

    public Guid AirportId { get; set; }
    public Airport Airport { get; set; } = default!;
}
