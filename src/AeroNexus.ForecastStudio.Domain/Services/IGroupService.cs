using AeroNexus.ForecastStudio.Domain.Entities;

namespace AeroNexus.ForecastStudio.Domain.Services;

public interface IGroupService
{
    Task<IReadOnlyList<GroupDefinition>> GetGroupsAsync(Guid scenarioId, CancellationToken cancellationToken = default);

    Task<GroupDefinition> CreateGroupAsync(Guid scenarioId, string name, string? description, bool isShared, IEnumerable<GroupCondition> conditions, CancellationToken cancellationToken = default);

    Task<GroupDefinition> UpdateGroupAsync(Guid groupId, string name, string? description, bool isShared, IEnumerable<GroupCondition> conditions, CancellationToken cancellationToken = default);

    Task DeleteGroupAsync(Guid groupId, CancellationToken cancellationToken = default);
}
