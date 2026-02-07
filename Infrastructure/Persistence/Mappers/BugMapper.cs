using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;

namespace Infrastructure.Persistence.Mapping;

public static class BugMapper
{
    public static Bug ToDomain(BugEntity entity, bool includeChildren = true)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = Bug.Rehydrate(
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
            stepsToReproduce: entity.StepsToReproduce,
            expectedResult: entity.ExpectedResult,
            actualResult: entity.ActualResult
        );

        WorkItemMappingHelpers.HydrateChildrenIfRequested(entity, domain, includeChildren);
        return domain;
    }

    public static BugEntity ToEntity(Bug domain)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));

        var entity = new BugEntity
        {
            StepsToReproduce = domain.StepsToReproduce,
            ExpectedResult = domain.ExpectedResult,
            ActualResult = domain.ActualResult
        };

        WorkItemMappingHelpers.FillEntityBaseFields(domain, entity);
        return entity;
    }

    public static void UpdateEntity(Bug domain, BugEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.StepsToReproduce = domain.StepsToReproduce;
        entity.ExpectedResult = domain.ExpectedResult;
        entity.ActualResult = domain.ActualResult;
    }
}