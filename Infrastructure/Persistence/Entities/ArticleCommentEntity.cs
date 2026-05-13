namespace Infrastructure.Persistence.Entities
{
    public sealed class ArticleCommentEntity : CommentEntity
    {
        public string ArticleId { get; set; } = null!;
        public ArticleEntity Article { get; set; } = null!;
    }
}
