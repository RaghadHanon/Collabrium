using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping;

public static class WorkItemMapper
{
    public static WorkItem ToDomain(WorkItemEntity entity, bool includeChildren = true)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        WorkItem domain = entity.Type switch
        {
            WorkItemType.Bug => BugMapper.ToDomain((BugEntity) entity, includeChildren),
            WorkItemType.Epic => EpicMapper.ToDomain((EpicEntity) entity, includeChildren),
            WorkItemType.TaskItem => TaskItemMapper.ToDomain((TaskItemEntity) entity, includeChildren),
            WorkItemType.UserStory => UserStoryMapper.ToDomain((UserStoryEntity) entity, includeChildren),
            WorkItemType.TestCase => TestCaseMapper.ToDomain((TestCaseEntity) entity, includeChildren),
            _ => throw new InvalidOperationException($"Unsupported work item type: {entity.Type}")
        };

        return domain;
    }

    public static WorkItemEntity ToEntity(WorkItem domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : domain.Type switch
            {
                WorkItemType.Bug => BugMapper.ToEntity((Bug) domain),
                WorkItemType.Epic => EpicMapper.ToEntity((Epic) domain),
                WorkItemType.TaskItem => TaskItemMapper.ToEntity((TaskItem) domain),
                WorkItemType.UserStory => UserStoryMapper.ToEntity((UserStory) domain),
                WorkItemType.TestCase => TestCaseMapper.ToEntity((TestCase) domain),
                _ => throw new InvalidOperationException($"Unsupported work item type: {domain.Type}")
            };
    }

    public static void UpdateEntity(WorkItem domain, WorkItemEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        // base shared fields
        entity.Title = domain.Title;
        entity.Description = domain.Description;
        entity.AcceptanceCriteria = domain.AcceptanceCriteria;
        entity.EffortPoints = domain.EffortPoints;

        entity.Type = domain.Type;
        entity.Status = domain.Status;
        entity.Priority = domain.Priority;

        entity.IsBlocked = domain.IsBlocked;
        entity.StartDate = domain.StartDate;
        entity.EndDate = domain.EndDate;
        entity.UpdatedDate = domain.UpdatedAt;

        entity.AssignedToUserId = domain.AssignedToUserId;
        entity.SprintId = domain.SprintId;

        // لا تغيّري CreatedByUserId/ProjectId عادةً بعد الإنشاء
        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;

        // subtype fields
        switch (domain.Type)
        {
            case WorkItemType.Bug:
                BugMapper.UpdateEntity((Bug) domain, (BugEntity) entity);
                break;
            case WorkItemType.UserStory:
                UserStoryMapper.UpdateEntity((UserStory) domain, (UserStoryEntity) entity);
                break;
            case WorkItemType.TestCase:
                TestCaseMapper.UpdateEntity((TestCase) domain, (TestCaseEntity) entity);
                break;
        }
    }
}