using AeroNexus.ForecastStudio.Domain.Entities;

namespace AeroNexus.ForecastStudio.Domain.Services;

public record ImportValidationResult(bool IsValid, IReadOnlyCollection<string> Errors);

public record ImportPreviewSummary(int NewFlights, int UpdatedFlights, int CancelledFlights, int DuplicateFlights);

public interface IImportService
{
    Task<ImportJob> CreateImportJobAsync(Guid scenarioId, string name, ImportType type, CancellationToken cancellationToken = default);

    Task<ImportValidationResult> ValidateStatisticalImportAsync(ImportJob job, Stream fileStream, string fileName, CancellationToken cancellationToken = default);

    Task<ImportValidationResult> ValidateScheduleImportAsync(ImportJob job, Stream fileStream, string fileName, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ImportColumnMapping>> AnalyseColumnsAsync(ImportJob job, Stream fileStream, CancellationToken cancellationToken = default);

    Task PersistColumnMappingsAsync(Guid importJobId, IEnumerable<ImportColumnMapping> mappings, CancellationToken cancellationToken = default);

    Task<ImportPreviewSummary> BuildSchedulePreviewAsync(Guid importJobId, Stream fileStream, DateOnly? targetStart, DateOnly? targetEnd, Guid? groupId, CancellationToken cancellationToken = default);

    Task<int> CommitStatisticalImportAsync(Guid importJobId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);

    Task<int> CommitScheduleImportAsync(Guid importJobId, Stream fileStream, DateOnly? targetStart, DateOnly? targetEnd, Guid? groupId, bool removeExisting, CancellationToken cancellationToken = default);

    Task<ImportJob> GetImportJobAsync(Guid importJobId, CancellationToken cancellationToken = default);
}
