using Domain.Entities;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping;

public static class ProjectMapper
{
    public static Project ToDomain(ProjectEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : Project.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            name: entity.Name,
            description: entity.Description,
            createdByUserId: entity.CreatedByUserId,
            teamId: entity.TeamId,
            tenantId: entity.TenantId
        );
    }

    public static ProjectEntity ToEntity(Project domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new ProjectEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt,

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

        entity.TeamId = domain.TeamId;
        entity.TenantId = domain.TenantId;

        // عادة CreatedByUserId ما بتتغير بعد الإنشاء
        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}