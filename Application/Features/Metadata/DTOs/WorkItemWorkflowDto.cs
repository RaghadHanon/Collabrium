using Domain.Enums;

namespace Application.Features.Metadata.DTOs;
public class WorkItemWorkflowDto
{
    public WorkItemType Type { get; set; }
    public List<WorkItemStatus> Workflow { get; set; } = new List<WorkItemStatus>();
}
