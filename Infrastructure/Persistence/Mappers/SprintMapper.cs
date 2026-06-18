using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class SprintMapper
{
    public static Sprint ToDomain(SprintEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return Sprint.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            name: entity.Name,
            code: entity.Code,
            goal: entity.Goal,
            tenantId: entity.TenantId,
            projectId: entity.ProjectId,
            plannedStartDate: entity.PlannedStartDate,
            plannedEndDate: entity.PlannedEndDate,
            startedAt: entity.StartedAt,
            completedAt: entity.CompletedAt,
            status: entity.Status);
    }

    public static SprintEntity ToEntity(Sprint domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new SprintEntity
        {
            Id = domain.Id,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt,

            TenantId = domain.TenantId,
            ProjectId = domain.ProjectId,

            Code = domain.Code,
            Name = domain.Name,
            Goal = domain.Goal,

            PlannedStartDate = domain.PlannedStartDate,
            PlannedEndDate = domain.PlannedEndDate,

            StartedAt = domain.StartedAt,
            CompletedAt = domain.CompletedAt,

            Status = domain.Status
        };
    }

    public static void UpdateEntity(
        Sprint domain,
        SprintEntity entity)
    {
        ArgumentNullException.ThrowIfNull(domain);
        ArgumentNullException.ThrowIfNull(entity);

        entity.TenantId = domain.TenantId;
        entity.ProjectId = domain.ProjectId;

        entity.Code = domain.Code;
        entity.Name = domain.Name;
        entity.Goal = domain.Goal;

        entity.PlannedStartDate = domain.PlannedStartDate;
        entity.PlannedEndDate = domain.PlannedEndDate;

        entity.StartedAt = domain.StartedAt;
        entity.CompletedAt = domain.CompletedAt;

        entity.Status = domain.Status;

        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}