using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class BugMapper
{
    public static Bug ToDomain(BugEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = new Bug(
            id: entity.Id,
            title: entity.Title,
            createdByUserId: entity.CreatedByUserId,
            projectId: entity.ProjectId,
            stepsToReproduce: entity.StepsToReproduce
        );

        domain.UpdateRepro(entity.StepsToReproduce, entity.ExpectedResult, entity.ActualResult);
        domain.UpdateDetails(entity.Title, entity.Description, entity.AcceptanceCriteria, entity.EffortPoints);
        domain.ChangeStatus(entity.Status);
        domain.SetPriority(entity.Priority);
        if (entity.IsBlocked) domain.Block(); else domain.Unblock();
        if (!string.IsNullOrWhiteSpace(entity.AssignedToUserId))
            domain.AssignTo(entity.AssignedToUserId!, entity.AssignedToUserName);
        domain.MoveToSprint(entity.SprintId);

        return domain;
    }

    public static BugEntity ToEntity(Bug domain)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));

        var entity = new BugEntity();
        UpdateEntity(domain, entity);
        return entity;
    }

    public static void UpdateEntity(Bug domain, BugEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        WorkItemMapper.MapCommonToEntity(domain, entity);

        entity.StepsToReproduce = domain.StepsToReproduce;
        entity.ExpectedResult = domain.ExpectedResult;
        entity.ActualResult = domain.ActualResult;
    }
}
