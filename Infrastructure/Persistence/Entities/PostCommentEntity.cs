namespace Infrastructure.Persistence.Entities
{
    public sealed class PostCommentEntity : CommentEntity
    {
        public string PostId { get; set; } = null!;
        public PostEntity Post { get; set; } = null!;
    }
}
