using Domain.Enums;
using Domain.ValueObjects;

namespace Infrastructure.Persistence.Entities;

public sealed class CourseEntity : DbEntity
{
    public string TenantId { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string Provider { get; set; } = null!;
    public string? InstructorName { get; set; }

    public CourseLevel Level { get; set; }
    public CourseLanguage Language { get; set; }

    public decimal? EstimatedDurationInHours { get; set; }

    public StoredImage? ThumbnailImage { get; set; }

    public TenantEntity Tenant { get; set; } = null!;

    public ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();
    public ICollection<UserCourseEntity> UserCourses { get; set; } = new List<UserCourseEntity>();
    public ICollection<CourseLearningPointEntity> LearningPoints { get; set; } = new List<CourseLearningPointEntity>();

    public ICollection<CourseRatingEntity> CourseRatings { get; set; } = new List<CourseRatingEntity>();
}
