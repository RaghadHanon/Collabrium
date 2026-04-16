using Domain.Enums;

namespace Application.Features.Courses.DTOs
{
    public class CourseDto
    {
        public required string Id { get; set; }

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

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
