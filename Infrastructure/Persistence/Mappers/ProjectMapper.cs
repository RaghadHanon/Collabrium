using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping;

public static class ProjectMapper
{
    public static Project ToDomain(ProjectEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return Project.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,

            name: entity.Name,
            description: entity.Description,

            status: entity.Status,
            startDate: entity.StartDate,
            endDate: entity.EndDate,

            createdByUserId: entity.CreatedByUserId,
            teamId: entity.TeamId,
            tenantId: entity.TenantId
        );
    }

    public static ProjectEntity ToEntity(Project domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new ProjectEntity
        {
            Id = domain.Id,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt,

            Name = domain.Name,
            Description = domain.Description,

            Status = domain.Status,
            StartDate = domain.StartDate,
            EndDate = domain.EndDate,

            CreatedByUserId = domain.CreatedByUserId,
            TeamId = domain.TeamId,
            TenantId = domain.TenantId
        };
    }

    public static void UpdateEntity(Project domain, ProjectEntity entity)
    {
        ArgumentNullException.ThrowIfNull(domain);
        ArgumentNullException.ThrowIfNull(entity);

        entity.Name = domain.Name;
        entity.Description = domain.Description;

        entity.Status = domain.Status;
        entity.StartDate = domain.StartDate;
        entity.EndDate = domain.EndDate;

        entity.TeamId = domain.TeamId;

        entity.TenantId = domain.TenantId;
        entity.CreatedByUserId = domain.CreatedByUserId;

        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}
