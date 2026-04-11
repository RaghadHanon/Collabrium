using Domain.Common;
using Domain.Enums;

namespace Domain.Models
{
    public sealed class UserCourse : Entity
    {
        public string TenantId { get; private set; } = null!;
        public string UserId { get; private set; } = null!;
        public string CourseId { get; private set; } = null!;

        public UserCourseStatus Status { get; private set; }
        public bool IsSaved { get; private set; }
        public bool IsAssigned { get; private set; }
        public string? AssignedByUserId { get; private set; }
        public DateTime? AssignedAt { get; private set; }
        public DateTime? StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        private UserCourse() { }

        public UserCourse(
            string tenantId,
            string userId,
            string courseId)
        {
            TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
            UserId = Guard.NotNullOrWhiteSpace(userId, nameof(userId));
            CourseId = Guard.NotNullOrWhiteSpace(courseId, nameof(courseId));

            Status = UserCourseStatus.NotStarted;
            IsSaved = false;
            IsAssigned = false;
        }

        public UserCourse(
            string id,
            string tenantId,
            string userId,
            string courseId) : base(id)
        {
            TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
            UserId = Guard.NotNullOrWhiteSpace(userId, nameof(userId));
            CourseId = Guard.NotNullOrWhiteSpace(courseId, nameof(courseId));

            Status = UserCourseStatus.NotStarted;
            IsSaved = false;
            IsAssigned = false;
        }

        public static UserCourse Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            string tenantId,
            string userId,
            string courseId,
            UserCourseStatus status,
            bool isSaved,
            bool isAssigned,
            string? assignedByUserId,
            DateTime? assignedAt,
            DateTime? startedAt,
            DateTime? completedAt)
        {
            var userCourse = new UserCourse(
                id: id,
                tenantId: tenantId,
                userId: userId,
                courseId: courseId);

            userCourse.SetTimestamps(createdAt, updatedAt);
            userCourse.Status = status;
            userCourse.IsSaved = isSaved;
            userCourse.IsAssigned = isAssigned;
            userCourse.AssignedByUserId = string.IsNullOrWhiteSpace(assignedByUserId) ? null : assignedByUserId.Trim();
            userCourse.AssignedAt = assignedAt;
            userCourse.StartedAt = startedAt;
            userCourse.CompletedAt = completedAt;
            return userCourse;
        }

        public void Assign(string assignedByUserId)
        {
            AssignedByUserId = Guard.NotNullOrWhiteSpace(assignedByUserId, nameof(assignedByUserId)).Trim();
            AssignedAt = DateTime.UtcNow;
            IsAssigned = true;
            Touch();
        }

        public void Unassign()
        {
            IsAssigned = false;
            AssignedByUserId = null;
            AssignedAt = null;
            Touch();
        }
        public void Start()
        {
            if (Status == UserCourseStatus.Completed)
                throw new DomainException("Completed course cannot be started again.");

            Status = UserCourseStatus.InProgress;

            if (!StartedAt.HasValue)
                StartedAt = DateTime.UtcNow;

            Touch();
        }

        public void Complete()
        {
            if (Status == UserCourseStatus.Completed)
                return;

            if (!StartedAt.HasValue)
                StartedAt = DateTime.UtcNow;

            Status = UserCourseStatus.Completed;
            CompletedAt = DateTime.UtcNow;

            Touch();
        }

        public void MarkAsSaved()
        {
            if (IsSaved)
                return;

            IsSaved = true;
            Touch();
        }

        public void RemoveFromSaved()
        {
            if (!IsSaved)
                return;

            IsSaved = false;
            Touch();
        }

        public void ResetProgress()
        {
            Status = UserCourseStatus.NotStarted;
            StartedAt = null;
            CompletedAt = null;

            Touch();
        }
        public bool CanBeDeletedWhenUnassigned()
        {
            return !IsSaved
                   && Status == UserCourseStatus.NotStarted
                   && !StartedAt.HasValue
                   && !CompletedAt.HasValue;
        }
    }
}
