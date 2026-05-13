namespace Application.Features.UserCourses.DTOs;

public sealed class AssignedByMeStatsDto
{
    public int TotalAssigned { get; set; }
    public int NotStartedCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
}
