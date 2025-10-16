namespace AeroNexus.ForecastStudio.Domain.Entities;

public enum ImportType
{
    StatisticalData,
    FlightSchedule
}

/// <summary>
/// Tracks the lifecycle of an import operation across wizard steps.
/// </summary>
public class ImportJob
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ScenarioId { get; set; }
    public Scenario Scenario { get; set; } = default!;

    public string Name { get; set; } = string.Empty;

    public ImportType Type { get; set; }
        = ImportType.StatisticalData;

    public string? SourceFileName { get; set; }
        = string.Empty;

    public ImportJobStatus Status { get; set; }
        = ImportJobStatus.Created;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAtUtc { get; set; }
        = null;

    public ICollection<ImportJobMessage> Messages { get; set; }
        = new List<ImportJobMessage>();

    public ICollection<ImportColumnMapping> ColumnMappings { get; set; }
        = new List<ImportColumnMapping>();

    public ICollection<ImportFlightCandidate> FlightCandidates { get; set; }
        = new List<ImportFlightCandidate>();
}

public enum ImportJobStatus
{
    Created,
    Validating,
    AwaitingMapping,
    AwaitingReview,
    Processing,
    Completed,
    Failed
}

public class ImportJobMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ImportJobId { get; set; }
    public ImportJob ImportJob { get; set; } = default!;

    public string Level { get; set; } = "Info";

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class ImportColumnMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ImportJobId { get; set; }
    public ImportJob ImportJob { get; set; } = default!;

    public string SourceColumn { get; set; } = string.Empty;

    public string TargetField { get; set; } = string.Empty;

    public bool IsRequired { get; set; }
        = false;
}

public class ImportFlightCandidate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ImportJobId { get; set; }
    public ImportJob ImportJob { get; set; } = default!;

    public string Category { get; set; } = string.Empty;

    public string FlightNumber { get; set; } = string.Empty;

    public string OriginAirportCode { get; set; } = string.Empty;

    public string DestinationAirportCode { get; set; } = string.Empty;

    public DateTime FlightDate { get; set; }
        = DateTime.UtcNow.Date;

    public bool Selected { get; set; }
        = true;

    public string? Details { get; set; }
        = string.Empty;
}
