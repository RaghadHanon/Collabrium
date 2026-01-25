using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class WorkItemMapper
{
    public static WorkItem ToDomain(WorkItemEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        if (entity is BugEntity bugEntity) return BugMapper.ToDomain(bugEntity);
        if (entity is TestCaseEntity testCaseEntity) return TestCaseMapper.ToDomain(testCaseEntity);
        if (entity is UserStoryEntity userStoryEntity) return UserStoryMapper.ToDomain(userStoryEntity);
        return entity is EpicEntity epicEntity
            ? EpicMapper.ToDomain(epicEntity)
            : entity is TaskItemEntity taskItemEntity
            ? (WorkItem) TaskItemMapper.ToDomain(taskItemEntity)
            : throw new NotSupportedException($"Unknown WorkItemEntity type: {entity.GetType().Name}");
    }

    public static WorkItemEntity ToEntity(WorkItem domain)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));

        if (domain is Bug bug) return BugMapper.ToEntity(bug);
        if (domain is TestCase tc) return TestCaseMapper.ToEntity(tc);
        if (domain is UserStory us) return UserStoryMapper.ToEntity(us);
        return domain is Epic epic
            ? (WorkItemEntity) EpicMapper.ToEntity(epic)
            : domain is TaskItem task
            ? (WorkItemEntity) TaskItemMapper.ToEntity(task)
            : throw new NotSupportedException($"Unknown WorkItem domain type: {domain.GetType().Name}");
    }

    internal static void MapCommonToEntity(WorkItem domain, WorkItemEntity entity)
    {
        entity.Id = domain.Id;
        entity.CreatedAt = domain.CreatedAt;

        entity.Title = domain.Title;
        entity.AcceptanceCriteria = domain.AcceptanceCriteria;
        entity.Description = domain.Description;
        entity.EffortPoints = domain.EffortPoints;

        entity.Type = domain.Type;
        entity.Status = domain.Status;
        entity.Priority = domain.Priority;

        entity.IsBlocked = domain.IsBlocked;
        entity.UpdatedDate = domain.UpdatedDate;
        entity.StartDate = domain.StartDate;
        entity.EndDate = domain.EndDate;

        entity.AssignedToUserId = domain.AssignedToUserId;
        entity.AssignedToUserName = domain.AssignedToUserName;

        entity.CreatedByUserId = domain.CreatedByUserId;
        entity.ProjectId = domain.ProjectId;
        entity.SprintId = domain.SprintId;
    }
}
