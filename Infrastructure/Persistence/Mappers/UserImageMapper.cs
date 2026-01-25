using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class UserImageMapper
{
    public static UserImage ToDomain(UserImageEntity entity)
    {
        return entity is null ? throw new ArgumentNullException(nameof(entity)) : new UserImage(entity.Url, entity.PublicId);
    }

    public static UserImageEntity ToEntity(UserImage domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new UserImageEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                Url = domain.Url,
                PublicId = domain.PublicId
            };
    }

    public static void UpdateEntity(UserImage domain, UserImageEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.Url = domain.Url;
        entity.PublicId = domain.PublicId;
    }
}
