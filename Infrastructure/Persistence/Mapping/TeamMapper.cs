using Domain.Entities;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping;

public static class TeamMapper
{
    public static Team ToDomain(TeamEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var memberIds =
            entity.Members?.Select(m => m.UserId) ??
            Enumerable.Empty<string>();

        return Team.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            tenantId: entity.TenantId,
            teamLeadId: entity.TeamLeadId,
            name: entity.Name,
            description: entity.Description,
            department: entity.Department,
            capacity: entity.Capacity,
            isActive: entity.IsActive,
            memberIds: memberIds
        );
    }

    public static TeamEntity ToEntity(Team domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new TeamEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = (DateTime) domain.UpdatedAt,

                TenantId = domain.TenantId,
                TeamLeadId = domain.TeamLeadId,
                Name = domain.Name,
                Description = domain.Description,
                Department = domain.Department,
                Capacity = domain.Capacity,
                IsActive = domain.IsActive,
            };
    }

    public static void UpdateEntity(Team domain, TeamEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.TenantId = domain.TenantId;
        entity.TeamLeadId = domain.TeamLeadId;

        entity.Name = domain.Name;
        entity.Description = domain.Description;
        entity.Department = domain.Department;

        entity.Capacity = domain.Capacity;
        entity.IsActive = domain.IsActive;

        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}