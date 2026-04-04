using Domain.Enums;

namespace Application.Features.UserCourses.DTOs
{
    public sealed class UserCourseDetailsDto
    {
        public required string Id { get; set; }

        public required string TenantId { get; set; }
        public required string UserId { get; set; }
        public required string CourseId { get; set; }

        public string? CourseTitle { get; set; }
        public string? CourseDescription { get; set; }
        public string? CourseUrl { get; set; }
        public string? Provider { get; set; }
        public string? InstructorName { get; set; }
        public string? CourseThumbnailUrl { get; set; }

        public UserCourseStatus Status { get; set; }
        public bool IsSaved { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
