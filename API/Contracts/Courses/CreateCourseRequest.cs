using Domain.Enums;

namespace API.Contracts.Courses;

public sealed class CreateCourseRequest
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Url { get; set; }
    public required string Provider { get; set; }
    public string? InstructorName { get; set; }

    public CourseLevel Level { get; set; }
    public CourseLanguage Language { get; set; }

    public int? EstimatedDurationInHours { get; set; }
    public List<string> LearningPoints { get; set; } = new();

}
