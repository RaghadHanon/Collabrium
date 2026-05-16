using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;
public static class AnnouncementAudienceMapper
{
    public static AnnouncementAudienceEntity ToEntity(AnnouncementAudience domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new AnnouncementAudienceEntity
        {
            Id = domain.Id,
            AnnouncementId = domain.AnnouncementId,
            Type = domain.Type,
            TeamId = domain.TeamId,
            RoleName = domain.RoleName,
            UserId = domain.UserId,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt
        };
    }

    public static AnnouncementAudience ToDomain(AnnouncementAudienceEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return AnnouncementAudience.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            announcementId: entity.AnnouncementId,
            type: entity.Type,
            teamId: entity.TeamId,
            roleName: entity.RoleName,
            userId: entity.UserId);
    }
}