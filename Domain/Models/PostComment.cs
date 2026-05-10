using Domain.Common;

namespace Domain.Models
{
    public sealed class PostComment : Comment
    {
        public string PostId { get; private set; } = null!;

        private PostComment() { }

        public PostComment(
            string postId,
            string createdByUserId,
            string content)
            : base(createdByUserId, content)
        {
            PostId = Guard.NotNullOrWhiteSpace(postId, nameof(postId)).Trim();
        }

        public PostComment(
            string id,
            string postId,
            string createdByUserId,
            string content)
            : base(id, createdByUserId, content)
        {
            PostId = Guard.NotNullOrWhiteSpace(postId, nameof(postId)).Trim();
        }

        public static PostComment Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            string postId,
            string createdByUserId,
            string content)
        {
            var comment = new PostComment(
                id,
                postId,
                createdByUserId,
                content);

            comment.SetTimestamps(createdAt, updatedAt);

            return comment;
        }
    }
}
