using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class TaskItemMapper
{
    public static TaskItem ToDomain(TaskItemEntity entity, bool includeChildren = true)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = TaskItem.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            title: entity.Title,
            createdByUserId: entity.CreatedByUserId,
            projectId: entity.ProjectId,
            status: entity.Status,
            priority: entity.Priority,
            isBlocked: entity.IsBlocked,
            effortPoints: entity.EffortPoints,
            description: entity.Description,
            acceptanceCriteria: entity.AcceptanceCriteria,
            assignedToUserId: entity.AssignedToUserId,
            sprintId: entity.SprintId,
            startDate: entity.StartDate,
            endDate: entity.EndDate
        );

        WorkItemMappingHelpers.HydrateChildrenIfRequested(entity, domain, includeChildren);
        return domain;
    }

    public static TaskItemEntity ToEntity(TaskItem domain)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));

        var entity = new TaskItemEntity();
        WorkItemMappingHelpers.FillEntityBaseFields(domain, entity);
        return entity;
    }
}