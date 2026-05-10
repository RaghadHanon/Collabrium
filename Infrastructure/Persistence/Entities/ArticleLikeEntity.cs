namespace Infrastructure.Persistence.Entities
{
    public sealed class ArticleLikeEntity : LikeEntity
    {
        public string ArticleId { get; set; } = null!;
        public ArticleEntity Article { get; set; } = null!;
    }
}
