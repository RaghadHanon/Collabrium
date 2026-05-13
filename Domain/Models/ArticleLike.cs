using Domain.Common;

namespace Domain.Models
{
    public sealed class ArticleLike : Entity
    {
        public string ArticleId { get; private set; } = null!;
        public string CreatedByUserId { get; private set; } = null!;

        private ArticleLike() { }

        public ArticleLike(string articleId, string createdByUserId)
        {
            ArticleId = Guard.NotNullOrWhiteSpace(articleId, nameof(articleId)).Trim();
            CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();

            Touch();
        }

        public ArticleLike(string id, string articleId, string createdByUserId) : base(id)
        {
            ArticleId = Guard.NotNullOrWhiteSpace(articleId, nameof(articleId)).Trim();
            CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();
        }

        public static ArticleLike Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            string articleId,
            string createdByUserId)
        {
            var like = new ArticleLike(id, articleId, createdByUserId);
            like.SetTimestamps(createdAt, updatedAt);
            return like;
        }
    }
}
