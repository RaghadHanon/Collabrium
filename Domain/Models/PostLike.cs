using Domain.Common;

namespace Domain.Models
{
    public sealed class PostLike : Like
    {
        public string PostId { get; private set; } = null!;

        private PostLike() { }

        public PostLike(
            string postId,
            string createdByUserId)
            : base(createdByUserId)
        {
            PostId = Guard.NotNullOrWhiteSpace(postId, nameof(postId)).Trim();
        }

        public PostLike(
            string id,
            string postId,
            string createdByUserId)
            : base(id, createdByUserId)
        {
            PostId = Guard.NotNullOrWhiteSpace(postId, nameof(postId)).Trim();
        }

        public static PostLike Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            string postId,
            string createdByUserId)
        {
            var like = new PostLike(id, postId, createdByUserId);

            like.SetTimestamps(createdAt, updatedAt);

            return like;
        }
    }
}
