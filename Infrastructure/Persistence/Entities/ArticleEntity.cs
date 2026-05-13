using Domain.ValueObjects;

namespace Infrastructure.Persistence.Entities
{
    public sealed class ArticleEntity : DbEntity
    {
        public string TenantId { get; set; } = null!;
        public TenantEntity Tenant { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public string Content { get; set; } = null!;

        public string CreatedByUserId { get; set; } = null!;
        public ApplicationUserEntity CreatedByUser { get; set; } = null!;

        public StoredImage? CoverImage { get; set; }

        public bool IsPublished { get; set; } = true;

        public ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();

        public ICollection<ArticleLikeEntity> Likes { get; set; } = new List<ArticleLikeEntity>();

        public ICollection<ArticleCommentEntity> Comments { get; set; } = new List<ArticleCommentEntity>();
    }
}
