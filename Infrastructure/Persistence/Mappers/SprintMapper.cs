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
            tenantId: entity.TenantId,
            projectId: entity.ProjectId,
            name: entity.Name,
            start: entity.StartDate,
            end: entity.EndDate,
            status: entity.Status
        );
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

            Name = domain.Name,
            StartDate = domain.StartDate,
            EndDate = domain.EndDate,
            Status = domain.Status
        };
    }

    public static void UpdateEntity(Sprint domain, SprintEntity entity)
    {
        ArgumentNullException.ThrowIfNull(domain);
        ArgumentNullException.ThrowIfNull(entity);

        entity.TenantId = domain.TenantId;
        entity.ProjectId = domain.ProjectId;

        entity.Name = domain.Name;
        entity.StartDate = domain.StartDate;
        entity.EndDate = domain.EndDate;
        entity.Status = domain.Status;

        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}