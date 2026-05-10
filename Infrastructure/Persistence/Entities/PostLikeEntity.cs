namespace Infrastructure.Persistence.Entities
{
    public sealed class PostLikeEntity : LikeEntity
    {
        public string PostId { get; set; } = null!;
        public PostEntity Post { get; set; } = null!;
    }
}
