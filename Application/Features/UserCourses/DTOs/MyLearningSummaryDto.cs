namespace Application.Features.UserCourses.DTOs;

public sealed class MyLearningSummaryDto
{
    public int AllCoursesCount { get; set; }
    public int SavedCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int AssignedToMeCount { get; set; }
}
