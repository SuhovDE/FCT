using System.Globalization;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using AeroNexus.ForecastStudio.Domain.Entities;
using AeroNexus.ForecastStudio.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroNexus.ForecastStudio.Infrastructure.Services;

public class ImportService : IImportService
{
    private static readonly string[] StatisticalRequiredColumns =
    {
        "airport_code",
        "period_start",
        "period_end",
        "total_movements",
        "total_passengers"
    };

    private readonly AeroNexusDbContext _dbContext;

    public ImportService(AeroNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ImportJob> CreateImportJobAsync(Guid scenarioId, string name, ImportType type, CancellationToken cancellationToken = default)
    {
        var job = new ImportJob
        {
            ScenarioId = scenarioId,
            Name = name,
            Type = type,
            Status = ImportJobStatus.Created
        };

        _dbContext.ImportJobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return job;
    }

    public async Task<ImportValidationResult> ValidateStatisticalImportAsync(ImportJob job, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
            && !fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Only CSV or ZIP archives are supported for statistical imports.");
            return new ImportValidationResult(false, errors);
        }

        await foreach (var record in ReadDelimitedRecordsAsync(fileStream, fileName, cancellationToken))
        {
            foreach (var required in StatisticalRequiredColumns)
            {
                if (!record.ContainsKey(required))
                {
                    errors.Add($"Missing required column '{required}'.");
                }
            }

            if (errors.Count > 0)
            {
                break;
            }

            if (!DateOnly.TryParse(record["period_start"], out _))
            {
                errors.Add("Unable to parse period_start as date.");
            }

            if (!DateOnly.TryParse(record["period_end"], out _))
            {
                errors.Add("Unable to parse period_end as date.");
            }

            if (!int.TryParse(record.GetValueOrDefault("total_movements"), out _))
            {
                errors.Add("Total movements must be an integer.");
            }

            if (!int.TryParse(record.GetValueOrDefault("total_passengers"), out _))
            {
                errors.Add("Total passengers must be an integer.");
            }

            break;
        }

        job.Status = errors.Count == 0 ? ImportJobStatus.AwaitingMapping : ImportJobStatus.Failed;
        job.SourceFileName = fileName;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ImportValidationResult(errors.Count == 0, errors);
    }

    public async Task<ImportValidationResult> ValidateScheduleImportAsync(ImportJob job, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
            && !fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Only CSV or ZIP archives are supported for flight schedule imports.");
        }

        var requiredColumns = new[] { "flight_number", "origin", "destination", "departure" };
        await foreach (var record in ReadDelimitedRecordsAsync(fileStream, fileName, cancellationToken))
        {
            foreach (var required in requiredColumns)
            {
                if (!record.ContainsKey(required))
                {
                    errors.Add($"Missing required column '{required}'.");
                }
            }

            if (record.TryGetValue("departure", out var departure)
                && !DateTime.TryParse(departure, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out _))
            {
                errors.Add("Departure column must contain valid date/time values.");
            }

            break;
        }

        job.SourceFileName = fileName;
        job.Status = errors.Count == 0 ? ImportJobStatus.AwaitingMapping : ImportJobStatus.Failed;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ImportValidationResult(errors.Count == 0, errors);
    }

    public async Task<IReadOnlyCollection<ImportColumnMapping>> AnalyseColumnsAsync(ImportJob job, Stream fileStream, CancellationToken cancellationToken = default)
    {
        var header = await ReadHeaderAsync(fileStream, job.SourceFileName ?? string.Empty, cancellationToken);
        var mappings = header
            .Select(column => new ImportColumnMapping
            {
                ImportJobId = job.Id,
                SourceColumn = column,
                TargetField = InferTargetField(column),
                IsRequired = StatisticalRequiredColumns.Contains(column, StringComparer.OrdinalIgnoreCase)
            })
            .ToList();

        return mappings;
    }

    public async Task PersistColumnMappingsAsync(Guid importJobId, IEnumerable<ImportColumnMapping> mappings, CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.ImportJobs
            .Include(import => import.ColumnMappings)
            .FirstOrDefaultAsync(import => import.Id == importJobId, cancellationToken)
            ?? throw new InvalidOperationException($"Import job {importJobId} not found");

        _dbContext.ImportColumnMappings.RemoveRange(job.ColumnMappings);
        job.ColumnMappings = mappings.Select(mapping => new ImportColumnMapping
        {
            SourceColumn = mapping.SourceColumn,
            TargetField = mapping.TargetField,
            IsRequired = mapping.IsRequired
        }).ToList();

        job.Status = ImportJobStatus.AwaitingReview;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ImportPreviewSummary> BuildSchedulePreviewAsync(Guid importJobId, Stream fileStream, DateOnly? targetStart, DateOnly? targetEnd, Guid? groupId, CancellationToken cancellationToken = default)
    {
        var job = await GetImportJobAsync(importJobId, cancellationToken);

        var group = groupId.HasValue
            ? await _dbContext.Groups
                .Include(g => g.Conditions)
                .FirstOrDefaultAsync(g => g.Id == groupId.Value, cancellationToken)
            : null;

        var scenarioFlights = await _dbContext.Flights
            .Include(flight => flight.Airline)
            .Where(flight => flight.ScenarioId == job.ScenarioId)
            .ToListAsync(cancellationToken);

        scenarioFlights = scenarioFlights
            .Where(flight => IsWithinRange(flight.DepartureTimeUtc, targetStart, targetEnd))
            .Where(flight => MatchesGroup(flight, group))
            .ToList();

        var flightLookup = scenarioFlights.ToDictionary(
            flight => CreateFlightKey(flight.FlightNumber, flight.DepartureTimeUtc),
            flight => flight);

        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var newFlights = 0;
        var updatedFlights = 0;
        var duplicates = 0;

        await foreach (var record in ReadDelimitedRecordsAsync(fileStream, job.SourceFileName ?? string.Empty, cancellationToken))
        {
            if (!record.TryGetValue("flight_number", out var flightNumber) || string.IsNullOrWhiteSpace(flightNumber))
            {
                continue;
            }

            if (!record.TryGetValue("departure", out var departureRaw)
                || !DateTime.TryParse(departureRaw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var departure))
            {
                continue;
            }

            if (!IsWithinRange(departure, targetStart, targetEnd))
            {
                continue;
            }

            if (!MatchesGroup(record, group))
            {
                continue;
            }

            var key = CreateFlightKey(flightNumber, departure);

            if (!seenKeys.Add(key))
            {
                duplicates++;
                continue;
            }

            if (!flightLookup.TryGetValue(key, out var existing))
            {
                newFlights++;
                continue;
            }

            var destination = record.GetValueOrDefault("destination") ?? string.Empty;
            var origin = record.GetValueOrDefault("origin") ?? string.Empty;
            var arrivalRaw = record.GetValueOrDefault("arrival");
            var arrivalMatches = arrivalRaw != null
                && DateTime.TryParse(arrivalRaw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var arrival)
                && Math.Abs((existing.ArrivalTimeUtc - arrival).TotalMinutes) < 1;

            if (!string.Equals(existing.DestinationAirportCode, destination, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(existing.OriginAirportCode, origin, StringComparison.OrdinalIgnoreCase)
                || !arrivalMatches)
            {
                updatedFlights++;
            }
        }

        var cancelledFlights = scenarioFlights
            .Select(flight => CreateFlightKey(flight.FlightNumber, flight.DepartureTimeUtc))
            .Count(key => !seenKeys.Contains(key));

        job.Status = ImportJobStatus.AwaitingReview;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ImportPreviewSummary(newFlights, updatedFlights, cancelledFlights, duplicates);
    }

    public async Task<int> CommitStatisticalImportAsync(Guid importJobId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var job = await GetImportJobAsync(importJobId, cancellationToken);

        var statistics = new List<AirportStatistic>();

        await foreach (var record in ReadDelimitedRecordsAsync(fileStream, fileName, cancellationToken))
        {
            if (!record.TryGetValue("airport_code", out var code))
            {
                continue;
            }

            var airport = await _dbContext.Airports
                .FirstOrDefaultAsync(a => a.IataCode == code, cancellationToken);

            if (airport is null)
            {
                airport = new Airport
                {
                    IataCode = code,
                    Name = code
                };

                _dbContext.Airports.Add(airport);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var statistic = new AirportStatistic
            {
                AirportId = airport.Id,
                ScenarioId = job.ScenarioId,
                PeriodStart = DateOnly.Parse(record["period_start"], CultureInfo.InvariantCulture),
                PeriodEnd = DateOnly.Parse(record["period_end"], CultureInfo.InvariantCulture),
                TotalMovements = int.Parse(record.GetValueOrDefault("total_movements") ?? "0", CultureInfo.InvariantCulture),
                TotalPassengers = int.Parse(record.GetValueOrDefault("total_passengers") ?? "0", CultureInfo.InvariantCulture),
                TotalFreightTonnes = int.Parse(record.GetValueOrDefault("total_freight_tonnes") ?? "0", CultureInfo.InvariantCulture),
                SourceFileName = fileName
            };

            statistics.Add(statistic);
        }

        await _dbContext.AirportStatistics.AddRangeAsync(statistics, cancellationToken);

        job.Status = ImportJobStatus.Completed;
        job.CompletedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return statistics.Count;
    }

    public async Task<int> CommitScheduleImportAsync(Guid importJobId, Stream fileStream, DateOnly? targetStart, DateOnly? targetEnd, Guid? groupId, bool removeExisting, CancellationToken cancellationToken = default)
    {
        var job = await GetImportJobAsync(importJobId, cancellationToken);

        var group = groupId.HasValue
            ? await _dbContext.Groups
                .Include(g => g.Conditions)
                .FirstOrDefaultAsync(g => g.Id == groupId.Value, cancellationToken)
            : null;

        var scenarioFlights = await _dbContext.Flights
            .Include(flight => flight.Airline)
            .Where(flight => flight.ScenarioId == job.ScenarioId)
            .ToListAsync(cancellationToken);

        var airlines = await _dbContext.Airlines.ToDictionaryAsync(airline => airline.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var airports = await _dbContext.Airports.ToDictionaryAsync(airport => airport.IataCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var flightLookup = scenarioFlights.ToDictionary(
            flight => CreateFlightKey(flight.FlightNumber, flight.DepartureTimeUtc),
            flight => flight);

        var processedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var processedFlights = 0;

        if (removeExisting)
        {
            var flightsToRemove = scenarioFlights
                .Where(flight => IsWithinRange(flight.DepartureTimeUtc, targetStart, targetEnd))
                .Where(flight => MatchesGroup(flight, group))
                .ToList();

            _dbContext.Flights.RemoveRange(flightsToRemove);
            foreach (var removed in flightsToRemove)
            {
                var key = CreateFlightKey(removed.FlightNumber, removed.DepartureTimeUtc);
                flightLookup.Remove(key);
            }
        }

        await foreach (var record in ReadDelimitedRecordsAsync(fileStream, job.SourceFileName ?? string.Empty, cancellationToken))
        {
            if (!record.TryGetValue("flight_number", out var flightNumber) || string.IsNullOrWhiteSpace(flightNumber))
            {
                continue;
            }

            if (!record.TryGetValue("departure", out var departureRaw)
                || !DateTime.TryParse(departureRaw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var departure))
            {
                continue;
            }

            if (!IsWithinRange(departure, targetStart, targetEnd))
            {
                continue;
            }

            if (!MatchesGroup(record, group))
            {
                continue;
            }

            var key = CreateFlightKey(flightNumber, departure);
            if (!processedKeys.Add(key))
            {
                continue;
            }

            var origin = record.GetValueOrDefault("origin") ?? string.Empty;
            var destination = record.GetValueOrDefault("destination") ?? string.Empty;
            var arrival = ParseDateTime(record.GetValueOrDefault("arrival"));
            var airlineCode = record.GetValueOrDefault("airline") ?? "UNK";
            var aircraftType = record.GetValueOrDefault("aircraft_type");

            if (!airlines.TryGetValue(airlineCode, out var airline))
            {
                airline = new Airline
                {
                    Code = airlineCode,
                    Name = airlineCode
                };

                _dbContext.Airlines.Add(airline);
                airlines[airlineCode] = airline;
            }

            if (!string.IsNullOrWhiteSpace(origin) && !airports.ContainsKey(origin))
            {
                var originAirport = new Airport { IataCode = origin, Name = origin };
                _dbContext.Airports.Add(originAirport);
                airports[origin] = originAirport;
            }

            if (!string.IsNullOrWhiteSpace(destination) && !airports.ContainsKey(destination))
            {
                var destinationAirport = new Airport { IataCode = destination, Name = destination };
                _dbContext.Airports.Add(destinationAirport);
                airports[destination] = destinationAirport;
            }

            if (flightLookup.TryGetValue(key, out var existing))
            {
                existing.OriginAirportCode = origin;
                existing.DestinationAirportCode = destination;
                existing.DepartureTimeUtc = departure;
                if (arrival is not null)
                {
                    existing.ArrivalTimeUtc = arrival.Value;
                }

                existing.AircraftType = aircraftType;
                existing.AirlineId = airline.Id;
            }
            else
            {
                var flight = new Flight
                {
                    ScenarioId = job.ScenarioId,
                    AirlineId = airline.Id,
                    FlightNumber = flightNumber,
                    OriginAirportCode = origin,
                    DestinationAirportCode = destination,
                    DepartureTimeUtc = departure,
                    ArrivalTimeUtc = arrival ?? departure,
                    AircraftType = aircraftType
                };

                _dbContext.Flights.Add(flight);
                flightLookup[key] = flight;
            }

            processedFlights++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        job.Status = ImportJobStatus.Completed;
        job.CompletedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return processedFlights;
    }

    public async Task<ImportJob> GetImportJobAsync(Guid importJobId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ImportJobs
            .Include(job => job.ColumnMappings)
            .Include(job => job.Messages)
            .FirstOrDefaultAsync(job => job.Id == importJobId, cancellationToken)
            ?? throw new InvalidOperationException($"Import job {importJobId} not found");
    }

    private static async Task<IReadOnlyList<string>> ReadHeaderAsync(Stream stream, string fileName, CancellationToken cancellationToken)
    {
        await using var copy = await CopyToMemoryStreamAsync(stream, cancellationToken);

        await foreach (var record in ReadDelimitedRecordsAsync(copy, fileName, cancellationToken))
        {
            return record.Keys.ToList();
        }

        return Array.Empty<string>();
    }

    private static async IAsyncEnumerable<Dictionary<string, string>> ReadDelimitedRecordsAsync(Stream stream, string fileName, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var copy = await CopyToMemoryStreamAsync(stream, cancellationToken);

        if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var archive = new ZipArchive(copy, ZipArchiveMode.Read, leaveOpen: false);
            var csvEntry = archive.Entries.FirstOrDefault(entry => entry.FullName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));
            if (csvEntry is null)
            {
                yield break;
            }

            await using var entryStream = csvEntry.Open();
            await foreach (var record in ReadCsvRecordsAsync(entryStream, cancellationToken))
            {
                yield return record;
            }
        }
        else
        {
            await foreach (var record in ReadCsvRecordsAsync(copy, cancellationToken))
            {
                yield return record;
            }
        }
    }

    private static async IAsyncEnumerable<Dictionary<string, string>> ReadCsvRecordsAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var headerLine = await reader.ReadLineAsync();
        if (headerLine is null)
        {
            yield break;
        }

        var headers = SplitCsvLine(headerLine);
        string? line;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = SplitCsvLine(line);
            var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < headers.Count; index++)
            {
                var header = headers[index];
                var value = index < values.Count ? values[index] : string.Empty;
                record[header] = value.Trim();
            }

            yield return record;
        }
    }

    private static List<string> SplitCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var insideQuotes = false;

        foreach (var character in line)
        {
            switch (character)
            {
                case '"':
                    insideQuotes = !insideQuotes;
                    break;
                case ',' when !insideQuotes:
                    values.Add(current.ToString());
                    current.Clear();
                    break;
                default:
                    current.Append(character);
                    break;
            }
        }

        values.Add(current.ToString());
        return values;
    }

    private static string InferTargetField(string column)
    {
        return column.ToLowerInvariant() switch
        {
            "airport" or "airport_code" or "iata" => "AirportCode",
            "period_start" => "PeriodStart",
            "period_end" => "PeriodEnd",
            "total_movements" => "TotalMovements",
            "total_passengers" => "TotalPassengers",
            "total_freight" or "total_freight_tonnes" => "TotalFreightTonnes",
            _ => column
        };
    }

    private static string CreateFlightKey(string flightNumber, DateTime departure)
    {
        return $"{flightNumber.Trim().ToUpperInvariant()}|{departure:yyyy-MM-dd}";
    }

    private static async Task<MemoryStream> CopyToMemoryStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (stream is MemoryStream memoryStream && memoryStream.CanSeek)
        {
            memoryStream.Position = 0;
            return memoryStream;
        }

        var copy = new MemoryStream();
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        await stream.CopyToAsync(copy, cancellationToken);
        copy.Position = 0;
        return copy;
    }

    private static bool IsWithinRange(DateTime departure, DateOnly? start, DateOnly? end)
    {
        var departureDate = DateOnly.FromDateTime(departure);
        if (start.HasValue && departureDate < start.Value)
        {
            return false;
        }

        if (end.HasValue && departureDate > end.Value)
        {
            return false;
        }

        return true;
    }

    private static bool MatchesGroup(Dictionary<string, string> record, GroupDefinition? group)
    {
        if (group is null || group.Conditions.Count == 0)
        {
            return true;
        }

        return group.Conditions.All(condition => EvaluateCondition(ResolveRecordValue(record, condition.Field), condition));
    }

    private static bool MatchesGroup(Flight flight, GroupDefinition? group)
    {
        if (group is null || group.Conditions.Count == 0)
        {
            return true;
        }

        return group.Conditions.All(condition => EvaluateCondition(ResolveFlightValue(flight, condition.Field), condition));
    }

    private static string? ResolveRecordValue(Dictionary<string, string> record, string field)
    {
        var key = field.ToLowerInvariant();
        return key switch
        {
            "airline.code" => record.GetValueOrDefault("airline"),
            "origin" or "origin.airport" => record.GetValueOrDefault("origin"),
            "destination" or "destination.airport" => record.GetValueOrDefault("destination"),
            _ => record.GetValueOrDefault(field.ToLowerInvariant())
        };
    }

    private static string? ResolveFlightValue(Flight flight, string field)
    {
        var key = field.ToLowerInvariant();
        return key switch
        {
            "airline.code" => flight.Airline?.Code,
            "origin" or "origin.airport" => flight.OriginAirportCode,
            "destination" or "destination.airport" => flight.DestinationAirportCode,
            _ => null
        };
    }

    private static bool EvaluateCondition(string? candidate, GroupCondition condition)
    {
        var comparison = condition.Value ?? string.Empty;
        return condition.Operator.ToLowerInvariant() switch
        {
            "equals" => string.Equals(candidate, comparison, StringComparison.OrdinalIgnoreCase),
            "contains" => candidate?.Contains(comparison, StringComparison.OrdinalIgnoreCase) == true,
            "in" => comparison
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(value => string.Equals(candidate, value, StringComparison.OrdinalIgnoreCase)),
            _ => true
        };
    }

    private static DateTime? ParseDateTime(string? value)
    {
        if (value is null)
        {
            return null;
        }

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed)
            ? parsed
            : null;
    }
}
