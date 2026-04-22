using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping
{
    public static class RoadmapMapper
    {
        public static Roadmap ToDomain(RoadmapEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var tagIds = entity.Tags?
                .Select(x => x.Id) ?? Enumerable.Empty<string>();

            return Roadmap.Rehydrate(
                id: entity.Id,
                createdAt: entity.CreatedAt,
                updatedAt: entity.UpdatedAt,
                path: entity.Path,
                level: entity.Level,
                title: entity.Title,
                description: entity.Description,
                publishedAt: entity.PublishedAt,
                IsVisible: entity.IsVisible,
                tenantId: entity.TenantId,
                createdByUserId: entity.CreatedByUserId,
                tagIds: tagIds
            );
        }

        public static RoadmapEntity ToEntity(Roadmap domain)
        {
            ArgumentNullException.ThrowIfNull(domain);

            return new RoadmapEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt,
                Path = domain.Path,
                Level = domain.Level,
                Title = domain.Title,
                Description = domain.Description,
                PublishedAt = domain.PublishedAt,
                IsVisible = domain.IsVisible,
                TenantId = domain.TenantId,
                CreatedByUserId = domain.CreatedByUserId,
            };
        }

        public static void UpdateEntity(Roadmap domain, RoadmapEntity entity)
        {
            ArgumentNullException.ThrowIfNull(domain);
            ArgumentNullException.ThrowIfNull(entity);

            entity.Path = domain.Path;
            entity.Level = domain.Level;
            entity.Title = domain.Title;
            entity.Description = domain.Description;
            entity.PublishedAt = domain.PublishedAt;
            entity.IsVisible = domain.IsVisible;
            entity.CreatedByUserId = domain.CreatedByUserId;
            entity.TenantId = domain.TenantId;
            entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
        }
    }
}
