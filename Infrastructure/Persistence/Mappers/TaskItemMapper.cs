using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class TaskItemMapper
{
    public static TaskItem ToDomain(TaskItemEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = new TaskItem(
            id: entity.Id,
            title: entity.Title,
            createdByUserId: entity.CreatedByUserId,
            projectId: entity.ProjectId
        );

        domain.UpdateDetails(entity.Title, entity.Description, entity.AcceptanceCriteria, entity.EffortPoints);
        domain.ChangeStatus(entity.Status);
        domain.SetPriority(entity.Priority);
        if (entity.IsBlocked) domain.Block(); else domain.Unblock();
        if (!string.IsNullOrWhiteSpace(entity.AssignedToUserId))
            domain.AssignTo(entity.AssignedToUserId!, entity.AssignedToUserName);
        domain.MoveToSprint(entity.SprintId);

        return domain;
    }

    public static TaskItemEntity ToEntity(TaskItem domain)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));

        var entity = new TaskItemEntity();
        UpdateEntity(domain, entity);
        return entity;
    }

    public static void UpdateEntity(TaskItem domain, TaskItemEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        WorkItemMapper.MapCommonToEntity(domain, entity);
    }
}
