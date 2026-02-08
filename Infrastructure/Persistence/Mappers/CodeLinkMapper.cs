using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class CodeLinkMapper
{
    public static CodeLink ToDomain(CodeLinkEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : CodeLink.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            url: entity.Url,
            linkType: entity.LinkType,
            displayName: entity.DisplayName
        );
    }

    public static CodeLinkEntity ToEntity(CodeLink domain, string workItemId)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new CodeLinkEntity
            {
                Id = domain.Id,
                WorkItemId = workItemId,
                Url = domain.Url,
                LinkType = domain.LinkType,
                DisplayName = domain.DisplayName,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
    }

    public static void UpdateEntity(CodeLink domain, CodeLinkEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.Url = domain.Url;
        entity.LinkType = domain.LinkType;
        entity.DisplayName = domain.DisplayName;
        entity.UpdatedAt = domain.UpdatedAt;
    }
}