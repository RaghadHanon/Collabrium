using Domain.Enums;

namespace API.Contracts.Courses;

public sealed class GetCoursesRequest
{
    public string? Search { get; set; }
    public string? Provider { get; set; }
    public string? InstructorName { get; set; }

    public CourseLevel? Level { get; set; }
    public CourseLanguage? Language { get; set; }

    public List<string>? TagIds { get; set; }

    public int? MinEstimatedDurationInHours { get; set; }
    public int? MaxEstimatedDurationInHours { get; set; }

    public bool? HasThumbnail { get; set; }
    public IReadOnlyCollection<string>? TagIdsAny { get; set; }
    public IReadOnlyCollection<string>? TagIdsAll { get; set; }
    public DateTime? Cursor { get; set; }
    public int PageSize { get; set; } = 10;
}
