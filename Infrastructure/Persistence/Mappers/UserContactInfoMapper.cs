using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class UserContactInfoMapper
{
    public static UserContactInfo ToDomain(UserContactInfoEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = new UserContactInfo();
        domain.UpdateSocials(entity.LinkedIn, entity.Facebook, entity.Instagram, entity.YouTube);
        domain.UpdateGithub(entity.Github);
        return domain;
    }

    public static UserContactInfoEntity ToEntity(UserContactInfo domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new UserContactInfoEntity
            {
                LinkedIn = domain.LinkedIn,
                Facebook = domain.Facebook,
                Instagram = domain.Instagram,
                YouTube = domain.YouTube,
                Github = domain.Github,
                CreatedAt = domain.CreatedAt,
                Id = domain.Id
            };
    }

    public static void UpdateEntity(UserContactInfo domain, UserContactInfoEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.LinkedIn = domain.LinkedIn;
        entity.Facebook = domain.Facebook;
        entity.Instagram = domain.Instagram;
        entity.YouTube = domain.YouTube;
        entity.Github = domain.Github;
    }
}
