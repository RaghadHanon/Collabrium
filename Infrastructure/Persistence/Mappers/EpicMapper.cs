using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class EpicMapper
{
    public static Epic ToDomain(EpicEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = new Epic(
            id: entity.Id,
            title: entity.Title,
            createdByUserId: entity.CreatedByUserId,
            projectId: entity.ProjectId
        );

        domain.UpdateDetails(entity.Title, entity.Description, entity.AcceptanceCriteria, entity.EffortPoints);
        domain.ChangeStatus(entity.Status);
        domain.SetPriority(entity.Priority);
        if (entity.IsBlocked) domain.Block(); else domain.Unblock();
        if (!string.IsNullOrWhiteSpace(entity.AssignedToUserId))
            domain.AssignTo(entity.AssignedToUserId!, entity.AssignedToUserName);
        domain.MoveToSprint(entity.SprintId);

        return domain;
    }

    public static EpicEntity ToEntity(Epic domain)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));

        var entity = new EpicEntity();
        UpdateEntity(domain, entity);
        return entity;
    }

    public static void UpdateEntity(Epic domain, EpicEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        WorkItemMapper.MapCommonToEntity(domain, entity);
    }
}
