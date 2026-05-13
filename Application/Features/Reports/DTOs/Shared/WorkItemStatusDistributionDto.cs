using Domain.Enums;

namespace Application.Features.Reports.DTOs.Shared;

public class WorkItemStatusDistributionDto
{
    public WorkItemStatus Status { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}