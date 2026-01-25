using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class UserStoryMapper
{
    public static UserStory ToDomain(UserStoryEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = new UserStory(
            id: entity.Id,
            title: entity.Title,
            createdByUserId: entity.CreatedByUserId,
            projectId: entity.ProjectId,
            storyPoints: entity.StoryPoints
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

    public static UserStoryEntity ToEntity(UserStory domain)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));

        var entity = new UserStoryEntity();
        UpdateEntity(domain, entity);
        return entity;
    }

    public static void UpdateEntity(UserStory domain, UserStoryEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        WorkItemMapper.MapCommonToEntity(domain, entity);
        entity.StoryPoints = domain.StoryPoints;
    }
}
