using Domain.Entities;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping;

public static class SprintMapper
{
    public static Sprint ToDomain(SprintEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : Sprint.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            projectId: entity.ProjectId,
            name: entity.Name,
            start: entity.StartDate,
            end: entity.EndDate,
            status: entity.Status
        );
    }

    public static SprintEntity ToEntity(Sprint domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new SprintEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt,

                ProjectId = domain.ProjectId,
                Name = domain.Name,
                StartDate = domain.StartDate,
                EndDate = domain.EndDate,
                Status = domain.Status
            };
    }

    public static void UpdateEntity(Sprint domain, SprintEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.Name = domain.Name;
        entity.StartDate = domain.StartDate;
        entity.EndDate = domain.EndDate;
        entity.Status = domain.Status;

        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}