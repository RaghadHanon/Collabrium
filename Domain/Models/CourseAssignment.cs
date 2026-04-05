using Domain.Common;

namespace Domain.Models
{
    public sealed class CourseAssignment : Entity
    {
        public string TenantId { get; private set; } = null!;
        public string CourseId { get; private set; } = null!;
        public string AssignedToUserId { get; private set; } = null!;
        public string AssignedByUserId { get; private set; } = null!;

        public DateTime AssignedAt { get; private set; }

        private CourseAssignment() { }

        public CourseAssignment(
            string tenantId,
            string courseId,
            string assignedToUserId,
            string assignedByUserId)
        {
            TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
            CourseId = Guard.NotNullOrWhiteSpace(courseId, nameof(courseId));
            AssignedToUserId = Guard.NotNullOrWhiteSpace(assignedToUserId, nameof(assignedToUserId));
            AssignedByUserId = Guard.NotNullOrWhiteSpace(assignedByUserId, nameof(assignedByUserId));

            AssignedAt = DateTime.UtcNow;
            Touch();
        }

        public CourseAssignment(
            string id,
            string tenantId,
            string courseId,
            string assignedToUserId,
            string assignedByUserId) : base(id)
        {
            TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
            CourseId = Guard.NotNullOrWhiteSpace(courseId, nameof(courseId));
            AssignedToUserId = Guard.NotNullOrWhiteSpace(assignedToUserId, nameof(assignedToUserId));
            AssignedByUserId = Guard.NotNullOrWhiteSpace(assignedByUserId, nameof(assignedByUserId));

            AssignedAt = DateTime.UtcNow;
        }

        public static CourseAssignment Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            string tenantId,
            string courseId,
            string assignedToUserId,
            string assignedByUserId,
            DateTime assignedAt)
        {
            var assignment = new CourseAssignment(
                id: id,
                tenantId: tenantId,
                courseId: courseId,
                assignedToUserId: assignedToUserId,
                assignedByUserId: assignedByUserId);

            assignment.SetTimestamps(createdAt, updatedAt);
            assignment.AssignedAt = assignedAt;

            return assignment;
        }
    }
}


