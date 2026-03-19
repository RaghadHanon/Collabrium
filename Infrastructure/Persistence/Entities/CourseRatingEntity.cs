namespace Infrastructure.Persistence.Entities
{
    public sealed class CourseRatingEntity : DbEntity
    {
        public string TenantId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string CourseId { get; set; } = null!;

        public int Rating { get; set; }
        public string? Review { get; set; }

        public TenantEntity Tenant { get; set; } = null!;
        public ApplicationUserEntity User { get; set; } = null!;
        public CourseEntity Course { get; set; } = null!;
    }
}
