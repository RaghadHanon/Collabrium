namespace Application.Features.Reports.DTOs;
public class SprintProgressDto
{
    public string SprintId { get; set; } = null!;
    public required string SprintCode { get; set; }
    public required string ProjectName { get; set; }
    public required string ProjectKey { get; set; }
    public int TotalWorkItems { get; set; }
    public int CompletedWorkItems { get; set; }

    // General progress percentage is calculated as (CompletedWorkItems / TotalWorkItems) * 100
    public double GeneralProgressPercentage { get; set; }

    // Progress percentage is calculated as (SumOfStoryPointsOfCompletedUserStory / SumOfStoryPointsOfTotalUserStory) * 100
    public double UserStoryProgressPercentage { get; set; }
}