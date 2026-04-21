namespace Application.Features.CourseRatings.DTOs
{
    public sealed class CourseRatingDetailsDto
    {
        public required string Id { get; set; }

        public required string TenantId { get; set; }
        public required string UserId { get; set; }
        public required string CourseId { get; set; }

        public int Rating { get; set; }

        public string? UserName { get; set; }
        public string? CourseTitle { get; set; }
        public string? CourseDescription { get; set; }
        public string? CourseThumbnailUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
