using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class TestCaseMapper
{
    public static TestCase ToDomain(TestCaseEntity entity, bool includeChildren = true)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = TestCase.Rehydrate(
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
            preconditions: entity.Preconditions,
            steps: entity.Steps,
            env: entity.TestEnvironment,
            expectedResult: entity.ExpectedResult,
            actualResult: entity.ActualResult,
            result: entity.Result
        );

        WorkItemMappingHelpers.HydrateChildrenIfRequested(entity, domain, includeChildren);
        return domain;
    }

    public static TestCaseEntity ToEntity(TestCase domain)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));

        var entity = new TestCaseEntity
        {
            Preconditions = domain.Preconditions,
            Steps = domain.Steps,
            TestEnvironment = domain.TestEnvironment,
            ExpectedResult = domain.ExpectedResult,
            ActualResult = domain.ActualResult,
            Result = domain.Result
        };

        WorkItemMappingHelpers.FillEntityBaseFields(domain, entity);
        return entity;
    }

    public static void UpdateEntity(TestCase domain, TestCaseEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.Preconditions = domain.Preconditions;
        entity.Steps = domain.Steps;
        entity.TestEnvironment = domain.TestEnvironment;
        entity.ExpectedResult = domain.ExpectedResult;
        entity.ActualResult = domain.ActualResult;
        entity.Result = domain.Result;
    }
}