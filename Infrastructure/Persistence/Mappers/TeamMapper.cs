using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class TeamMapper
{
    public static Team ToDomain(TeamEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = new Team(
            id: entity.Id,
            tenantId: entity.TenantId,
            teamLeadId: entity.TeamLeadId,
            name: entity.Name,
            department: entity.Department,
            isActive: entity.IsActive
        );

        domain.UpdateDetails(entity.Name, entity.Description, entity.Capacity);

        return domain;
    }

    public static TeamEntity ToEntity(Team domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new TeamEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                TenantId = domain.TenantId,
                TeamLeadId = domain.TeamLeadId,
                Name = domain.Name,
                Description = domain.Description,
                Department = domain.Department,
                Capacity = domain.Capacity,
                IsActive = domain.IsActive,
                UpdatedAt = domain.UpdatedAt
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
        entity.UpdatedAt = domain.UpdatedAt;
    }
}
