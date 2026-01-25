using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class ProjectMapper
{
    public static Project ToDomain(ProjectEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = new Project(
            id: entity.Id,
            name: entity.Name,
            createdByUserId: entity.CreatedByUserId,
            teamId: entity.TeamId,
            tenantId: entity.TenantId
        );

        domain.Rename(entity.Name, entity.Description);
        return domain;
    }

    public static ProjectEntity ToEntity(Project domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new ProjectEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                Name = domain.Name,
                Description = domain.Description,
                CreatedByUserId = domain.CreatedByUserId,
                TeamId = domain.TeamId,
                TenantId = domain.TenantId
            };
    }

    public static void UpdateEntity(Project domain, ProjectEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.Name = domain.Name;
        entity.Description = domain.Description;
        entity.CreatedByUserId = domain.CreatedByUserId;
        entity.TeamId = domain.TeamId;
        entity.TenantId = domain.TenantId;
    }
}
