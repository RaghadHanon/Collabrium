namespace Application.Features.Reports.DTOs;

public class VelocityReportDto
{
    public required string ProjectId { get; set; }
    public required string ProjectKey { get; set; }
    public required string ProjectName { get; set; }
    public int TotalStoryPoints { get; set; } = 0;
    public int CompletedStoryPoints { get; set; } = 0;
    public double AvgVelocityPercentage { get; set; } = 0.0;
    public List<SprintVelocityDto> SprintVelocities { get; set; } = new List<SprintVelocityDto>();
}

public class SprintVelocityDto
{
    public required string SprintId { get; set; }
    public required string SprintCode { get; set; }
    public int TotalStoryPoints { get; set; } = 0;
    public int CompletedStoryPoints { get; set; } = 0;
    // Velocity percentage is calculated as (CompletedStoryPoints / TotalStoryPoints) * 100
    public double VelocityPercentage { get; set; } = 0.0;
}