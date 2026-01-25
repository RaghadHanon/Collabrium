using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class TenantImageMapper
{
    public static TenantImage ToDomain(TenantImageEntity entity)
    {
        return entity is null ? throw new ArgumentNullException(nameof(entity)) : new TenantImage(entity.Url, entity.PublicId);
    }

    public static TenantImageEntity ToEntity(TenantImage domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new TenantImageEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                Url = domain.Url,
                PublicId = domain.PublicId
            };
    }

    public static void UpdateEntity(TenantImage domain, TenantImageEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.Url = domain.Url;
        entity.PublicId = domain.PublicId;
    }
}
