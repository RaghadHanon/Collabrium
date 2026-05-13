using Domain.Common;

namespace Domain.Models
{
    public sealed class ArticleComment : Entity
    {
        public string ArticleId { get; private set; } = null!;
        public string Content { get; private set; } = null!;
        public string CreatedByUserId { get; private set; } = null!;

        private ArticleComment() { }

        public ArticleComment(
            string articleId,
            string content,
            string createdByUserId)
        {
            ArticleId = Guard.NotNullOrWhiteSpace(articleId, nameof(articleId)).Trim();
            Content = Guard.NotNullOrWhiteSpace(content, nameof(content)).Trim();
            CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();

            Touch();
        }

        public ArticleComment(
            string id,
            string articleId,
            string content,
            string createdByUserId) : base(id)
        {
            ArticleId = Guard.NotNullOrWhiteSpace(articleId, nameof(articleId)).Trim();
            Content = Guard.NotNullOrWhiteSpace(content, nameof(content)).Trim();
            CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();
        }

        public static ArticleComment Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            string articleId,
            string content,
            string createdByUserId)
        {
            var comment = new ArticleComment(id, articleId, content, createdByUserId);
            comment.SetTimestamps(createdAt, updatedAt);
            return comment;
        }

        public void UpdateContent(string content)
        {
            Content = Guard.NotNullOrWhiteSpace(content, nameof(content)).Trim();
            Touch();
        }
    }
}
