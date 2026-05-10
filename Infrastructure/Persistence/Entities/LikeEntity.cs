namespace Infrastructure.Persistence.Entities
{
    public class LikeEntity : DbEntity
    {
        public string CreatedByUserId { get; set; } = null!;
        public ApplicationUserEntity CreatedByUser { get; set; } = null!;
    }
}
