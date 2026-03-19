using Domain.Enums;

namespace Infrastructure.Persistence.Entities
{
    public sealed class UserCourseEntity : DbEntity
    {
        public string TenantId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string CourseId { get; set; } = null!;

        public UserCourseStatus Status { get; set; }
        public bool IsSaved { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public TenantEntity Tenant { get; set; } = null!;
        public ApplicationUserEntity User { get; set; } = null!;
        public CourseEntity Course { get; set; } = null!;
    }
}
