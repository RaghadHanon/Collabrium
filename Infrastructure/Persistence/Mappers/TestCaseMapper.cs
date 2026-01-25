using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class TestCaseMapper
{
    public static TestCase ToDomain(TestCaseEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = new TestCase(
            id: entity.Id,
            title: entity.Title,
            createdByUserId: entity.CreatedByUserId,
            projectId: entity.ProjectId,
            preconditions: entity.Preconditions,
            steps: entity.Steps,
            env: entity.TestEnvironment,
            expectedResult: entity.ExpectedResult
        );

        // Base fields via behavior methods:
        domain.UpdateDetails(entity.Title, entity.Description, entity.AcceptanceCriteria, entity.EffortPoints);
        domain.ChangeStatus(entity.Status);
        domain.SetPriority(entity.Priority);
        if (entity.IsBlocked) domain.Block(); else domain.Unblock();
        if (!string.IsNullOrWhiteSpace(entity.AssignedToUserId))
            domain.AssignTo(entity.AssignedToUserId!, entity.AssignedToUserName);
        domain.MoveToSprint(entity.SprintId);

        return domain;
    }

    public static TestCaseEntity ToEntity(TestCase domain)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));

        var entity = new TestCaseEntity();
        UpdateEntity(domain, entity);
        return entity;
    }

    public static void UpdateEntity(TestCase domain, TestCaseEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        WorkItemMapper.MapCommonToEntity(domain, entity);

        entity.Preconditions = domain.Preconditions;
        entity.Steps = domain.Steps;
        entity.TestEnvironment = domain.TestEnvironment;
        entity.ExpectedResult = domain.ExpectedResult;
        entity.ActualResult = domain.ActualResult;
    }
}
