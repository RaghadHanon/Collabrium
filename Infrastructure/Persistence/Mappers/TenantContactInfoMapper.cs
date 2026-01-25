using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class TenantContactInfoMapper
{
    public static TenantContactInfo ToDomain(TenantContactInfoEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = new TenantContactInfo();
        domain.UpdateSocials(entity.LinkedIn, entity.Facebook, entity.Instagram, entity.YouTube);
        domain.UpdateContact(entity.Email, entity.PhoneNumber);
        return domain;
    }

    public static TenantContactInfoEntity ToEntity(TenantContactInfo domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new TenantContactInfoEntity
            {
                LinkedIn = domain.LinkedIn,
                Facebook = domain.Facebook,
                Instagram = domain.Instagram,
                YouTube = domain.YouTube,
                Email = domain.Email,
                PhoneNumber = domain.PhoneNumber,
                CreatedAt = domain.CreatedAt,
                Id = domain.Id
            };
    }

    public static void UpdateEntity(TenantContactInfo domain, TenantContactInfoEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.LinkedIn = domain.LinkedIn;
        entity.Facebook = domain.Facebook;
        entity.Instagram = domain.Instagram;
        entity.YouTube = domain.YouTube;
        entity.Email = domain.Email;
        entity.PhoneNumber = domain.PhoneNumber;
    }
}
