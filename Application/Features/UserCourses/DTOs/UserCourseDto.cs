using Domain.Enums;

namespace Application.Features.UserCourses.DTOs
{
    public sealed class UserCourseDto
    {
        public required string Id { get; set; }

        public required string TenantId { get; set; }
        public required string UserId { get; set; }
        public required string CourseId { get; set; }

        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Url { get; set; }
        public required string Provider { get; set; }
        public string? InstructorName { get; set; }

        public CourseLevel Level { get; set; }
        public CourseLanguage Language { get; set; }

        public int? EstimatedDurationInHours { get; set; }

        public string? ThumbnailImageUrl { get; set; }
        public string? ThumbnailImagePublicId { get; set; }

        public List<string> TagIds { get; set; } = new();

        public int RatingsCount { get; set; }
        public double AverageRating { get; set; }

        public UserCourseStatus Status { get; set; }

        public bool IsSaved { get; set; }

        public bool IsAssigned { get; set; }
        public string? AssignedByUserId { get; set; }
        public string? AssignedByUserName { get; set; }
        public DateTime? AssignedAt { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
