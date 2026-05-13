using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class TagMapper
{
    public static TagEntity ToEntity(Tag domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new TagEntity
        {
            Id = domain.Id,
            TenantId = domain.TenantId,
            Name = domain.Name,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt
        };
    }
    public static Tag ToDomain(TagEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return Tag.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            tenantId: entity.TenantId,
            name: entity.Name
        );
    }
    public static void UpdateEntity(Tag domain, TagEntity entity)
    {
        ArgumentNullException.ThrowIfNull(domain);
        ArgumentNullException.ThrowIfNull(entity);

        entity.Name = domain.Name;
        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}
