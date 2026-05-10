using Domain.Models;
using Domain.ValueObjects;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers
{
    public static class PostMapper
    {
        public static PostEntity ToEntity(Post post)
        {
            ArgumentNullException.ThrowIfNull(post);

            return new PostEntity
            {
                Id = post.Id,
                TenantId = post.TenantId,
                CreatedByUserId = post.CreatedByUserId,
                Title = post.Title,
                Content = post.Content,
                Type = post.Type,
                IsPinned = post.IsPinned,
                PinnedByUserId = post.PinnedByUserId,
                PinnedAt = post.PinnedAt,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,

                Links = post.Links
                    .Select(x => new PostLinkOwned { Url = x.Url })
                    .ToList(),

                Images = post.Images
                    .Select(x => new StoredImageOwned
                    {
                        Url = x.Url,
                        PublicId = x.PublicId
                    })
                    .ToList()
            };
        }

        public static Post ToDomain(PostEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return Post.Rehydrate(
                id: entity.Id,
                createdAt: entity.CreatedAt,
                updatedAt: entity.UpdatedAt,
                tenantId: entity.TenantId,
                createdByUserId: entity.CreatedByUserId,
                title: entity.Title,
                content: entity.Content,
                type: entity.Type,
                isPinned: entity.IsPinned,
                pinnedByUserId: entity.PinnedByUserId,
                pinnedAt: entity.PinnedAt,
                tagIds: entity.Tags.Select(x => x.Id),
                links: entity.Links.Select(x => PostLink.Create(x.Url)),
                images: entity.Images.Select(x => StoredImage.Create(x.Url, x.PublicId))
            );
        }

        public static void UpdateEntity(PostEntity entity, Post post)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(post);

            entity.Title = post.Title;
            entity.Content = post.Content;
            entity.Type = post.Type;
            entity.IsPinned = post.IsPinned;
            entity.PinnedByUserId = post.PinnedByUserId;
            entity.PinnedAt = post.PinnedAt;
            entity.UpdatedAt = post.UpdatedAt ?? DateTime.UtcNow;

            entity.Links.Clear();
            foreach (var link in post.Links)
                entity.Links.Add(new PostLinkOwned { Url = link.Url });

            entity.Images.Clear();
            foreach (var image in post.Images)
                entity.Images.Add(new StoredImageOwned
                {
                    Url = image.Url,
                    PublicId = image.PublicId
                });
        }
    }
}
