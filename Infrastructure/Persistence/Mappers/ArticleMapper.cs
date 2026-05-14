using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers
{
    public static class ArticleMapper
    {
        public static ArticleEntity ToEntity(Article domain)
        {
            ArgumentNullException.ThrowIfNull(domain);

            return new ArticleEntity
            {
                Id = domain.Id,
                TenantId = domain.TenantId,
                Title = domain.Title,
                Description = domain.Description,
                Content = domain.Content,
                CreatedByUserId = domain.CreatedByUserId,
                CoverImage = domain.CoverImage,
                IsPublished = domain.IsPublished,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }

        public static Article ToDomain(ArticleEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return Article.Rehydrate(
                id: entity.Id,
                createdAt: entity.CreatedAt,
                updatedAt: entity.UpdatedAt,
                tenantId: entity.TenantId,
                title: entity.Title,
                description: entity.Description,
                content: entity.Content,
                createdByUserId: entity.CreatedByUserId,
                coverImage: entity.CoverImage,
                isPublished: entity.IsPublished,
                tagIds: entity.Tags?.Select(t => t.Id)
            );
        }

        public static void UpdateEntity(Article domain, ArticleEntity entity)
        {
            ArgumentNullException.ThrowIfNull(domain);
            ArgumentNullException.ThrowIfNull(entity);

            entity.Title = domain.Title;
            entity.Description = domain.Description;
            entity.Content = domain.Content;
            entity.CoverImage = domain.CoverImage;
            entity.IsPublished = domain.IsPublished;
            entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
        }
    }
}
