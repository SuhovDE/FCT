namespace AeroNexus.ForecastStudio.Domain.Entities;

/// <summary>
/// Represents a single scheduled or generated flight event within a scenario.
/// </summary>
public class Flight
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ScenarioId { get; set; }
    public Scenario Scenario { get; set; } = default!;

    public Guid AirlineId { get; set; }
    public Airline Airline { get; set; } = default!;

    public string FlightNumber { get; set; } = string.Empty;

    public string OriginAirportCode { get; set; } = string.Empty;

    public string DestinationAirportCode { get; set; } = string.Empty;

    public DateTime DepartureTimeUtc { get; set; }
        = DateTime.UtcNow;

    public DateTime ArrivalTimeUtc { get; set; }
        = DateTime.UtcNow;

    public string? AircraftType { get; set; }
        = string.Empty;

    public bool IsLinked { get; set; }
        = false;

    public Guid? LinkedFlightId { get; set; }
        = null;

    public bool IsCancelled { get; set; }
        = false;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
