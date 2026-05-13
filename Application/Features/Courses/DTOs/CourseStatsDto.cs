namespace Application.Features.Courses.DTOs;

public sealed class CourseStatsDto
{
    public int SavedCount { get; set; }
    public int AssignedCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
}
