using Domain.Enums;

namespace Application.Features.Reports.DTOs.Shared;

public class WorkItemPriorityDistributionDto
{
    public WorkItemPriority Priority { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}