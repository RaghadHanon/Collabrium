namespace Infrastructure.Persistence.Entities
{
    public sealed class ArticleLikeEntity : DbEntity
    {
        public string ArticleId { get; set; } = null!;
        public ArticleEntity Article { get; set; } = null!;

        public string CreatedByUserId { get; set; } = null!;
        public ApplicationUserEntity CreatedByUser { get; set; } = null!;
    }
}
