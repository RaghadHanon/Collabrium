namespace Infrastructure.Persistence.Entities
{
    public sealed class CourseAssignmentEntity : DbEntity
    {
        public string TenantId { get; set; } = null!;
        public string CourseId { get; set; } = null!;
        public string AssignedToUserId { get; set; } = null!;
        public string AssignedByUserId { get; set; } = null!;

        public DateTime AssignedAt { get; set; }

        public TenantEntity Tenant { get; set; } = null!;
        public CourseEntity Course { get; set; } = null!;
        public ApplicationUserEntity AssignedToUser { get; set; } = null!;
        public ApplicationUserEntity AssignedByUser { get; set; } = null!;
    }
}
