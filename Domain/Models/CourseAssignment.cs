using Domain.Common;
using Domain.Enums;

namespace Domain.Models
{
    public sealed class CourseAssignment : Entity
    {
        public string TenantId { get; private set; } = null!;
        public string CourseId { get; private set; } = null!;
        public string AssignedToUserId { get; private set; } = null!;
        public string AssignedByUserId { get; private set; } = null!;

        public DateTime AssignedAt { get; private set; }

        public CourseAssignmentStatus Status { get; private set; }

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
            Status = CourseAssignmentStatus.Active;
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
            Status = CourseAssignmentStatus.Active;
        }

        public static CourseAssignment Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            string tenantId,
            string courseId,
            string assignedToUserId,
            string assignedByUserId,
            DateTime assignedAt,
            CourseAssignmentStatus status)
        {
            var assignment = new CourseAssignment(
                id: id,
                tenantId: tenantId,
                courseId: courseId,
                assignedToUserId: assignedToUserId,
                assignedByUserId: assignedByUserId);

            assignment.SetTimestamps(createdAt, updatedAt);
            assignment.AssignedAt = assignedAt;
            assignment.Status = status;

            return assignment;
        }

        // ============================
        // Behavior
        // ============================

        public void MarkAsCompleted()
        {
            if (Status == CourseAssignmentStatus.Cancelled)
                throw new DomainException("Cancelled assignment cannot be completed.");

            if (Status == CourseAssignmentStatus.Completed)
                return;

            Status = CourseAssignmentStatus.Completed;
            Touch();
        }

        public void Cancel()
        {
            if (Status == CourseAssignmentStatus.Completed)
                throw new DomainException("Completed assignment cannot be cancelled.");

            if (Status == CourseAssignmentStatus.Cancelled)
                return;

            Status = CourseAssignmentStatus.Cancelled;
            Touch();
        }

        public void ReActivate()
        {
            if (Status == CourseAssignmentStatus.Completed)
                throw new DomainException("Completed assignment cannot be re-activated.");

            if (Status == CourseAssignmentStatus.Active)
                return;

            Status = CourseAssignmentStatus.Active;
            Touch();
        }
    }

}
