using AeroNexus.ForecastStudio.Domain.Entities;
using AeroNexus.ForecastStudio.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroNexus.ForecastStudio.Infrastructure.Services;

public class GroupService : IGroupService
{
    private readonly AeroNexusDbContext _dbContext;

    public GroupService(AeroNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<GroupDefinition>> GetGroupsAsync(Guid scenarioId, CancellationToken cancellationToken = default)
    {
        var groups = await _dbContext.Groups
            .Include(group => group.Conditions)
            .Where(group => group.ScenarioId == scenarioId)
            .OrderBy(group => group.Name)
            .ToListAsync(cancellationToken);

        foreach (var group in groups)
        {
            group.Conditions = group.Conditions
                .OrderBy(condition => condition.Order)
                .ToList();
        }

        return groups;
    }

    public async Task<GroupDefinition> CreateGroupAsync(Guid scenarioId, string name, string? description, bool isShared, IEnumerable<GroupCondition> conditions, CancellationToken cancellationToken = default)
    {
        var group = new GroupDefinition
        {
            ScenarioId = scenarioId,
            Name = name,
            Description = description,
            IsShared = isShared,
            Conditions = conditions
                .Select((condition, index) => new GroupCondition
                {
                    GroupDefinitionId = Guid.Empty,
                    Field = condition.Field,
                    Operator = condition.Operator,
                    Value = condition.Value,
                    Order = index
                })
                .ToList()
        };

        foreach (var condition in group.Conditions)
        {
            condition.GroupDefinitionId = group.Id;
        }

        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return group;
    }

    public async Task<GroupDefinition> UpdateGroupAsync(Guid groupId, string name, string? description, bool isShared, IEnumerable<GroupCondition> conditions, CancellationToken cancellationToken = default)
    {
        var existingGroup = await _dbContext.Groups
            .Include(group => group.Conditions)
            .FirstOrDefaultAsync(group => group.Id == groupId, cancellationToken)
            ?? throw new InvalidOperationException($"Group {groupId} was not found");

        existingGroup.Name = name;
        existingGroup.Description = description;
        existingGroup.IsShared = isShared;

        _dbContext.GroupConditions.RemoveRange(existingGroup.Conditions);
        existingGroup.Conditions = conditions
            .Select((condition, index) => new GroupCondition
            {
                GroupDefinitionId = existingGroup.Id,
                Field = condition.Field,
                Operator = condition.Operator,
                Value = condition.Value,
                Order = index
            })
            .ToList();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return existingGroup;
    }

    public async Task DeleteGroupAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(entity => entity.Id == groupId, cancellationToken)
            ?? throw new InvalidOperationException($"Group {groupId} was not found");

        _dbContext.Groups.Remove(group);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
