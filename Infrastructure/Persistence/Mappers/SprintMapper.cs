using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class SprintMapper
{
    public static Sprint ToDomain(SprintEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = new Sprint(entity.Id, entity.ProjectId, entity.Name, entity.StartDate, entity.EndDate);

        // If you want exact status from DB, add a domain method like SetStatus(status)
        // and call it here.
        // For now: leave domain default or evolve it through behavior.
        return domain;
    }

    public static SprintEntity ToEntity(Sprint domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new SprintEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                Name = domain.Name,
                StartDate = domain.StartDate,
                EndDate = domain.EndDate,
                Status = domain.Status,
                ProjectId = domain.ProjectId
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
        entity.ProjectId = domain.ProjectId;
    }
}
