using Domain.Entities;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping;

public static class UserStoryMapper
{
    public static UserStory ToDomain(UserStoryEntity entity, bool includeChildren = true)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = UserStory.Rehydrate(
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
            endDate: entity.EndDate,
            storyPoints: entity.StoryPoints
        );

        WorkItemMappingHelpers.HydrateChildrenIfRequested(entity, domain, includeChildren);
        return domain;
    }

    public static UserStoryEntity ToEntity(UserStory domain)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));

        var entity = new UserStoryEntity
        {
            StoryPoints = domain.StoryPoints
        };

        WorkItemMappingHelpers.FillEntityBaseFields(domain, entity);
        return entity;
    }

    public static void UpdateEntity(UserStory domain, UserStoryEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.StoryPoints = domain.StoryPoints;
    }
}