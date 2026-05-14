using Domain.Enums;

namespace Infrastructure.Persistence.Entities;

public sealed class PostEntity : DbEntity
{
    public string TenantId { get; set; } = null!;
    public TenantEntity Tenant { get; set; } = null!;

    public string CreatedByUserId { get; set; } = null!;
    public ApplicationUserEntity CreatedByUser { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public PostType Type { get; set; }

    public bool IsPinned { get; set; }
    public string? PinnedByUserId { get; set; }
    public ApplicationUserEntity? PinnedByUser { get; set; }
    public DateTime? PinnedAt { get; set; }

    public ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();
    public ICollection<PostLinkOwned> Links { get; set; } = new List<PostLinkOwned>();
    public ICollection<StoredImageOwned> Images { get; set; } = new List<StoredImageOwned>();
    public ICollection<PostLikeEntity> Likes { get; set; } = new List<PostLikeEntity>();
    public ICollection<PostCommentEntity> Comments { get; set; } = new List<PostCommentEntity>();
}
