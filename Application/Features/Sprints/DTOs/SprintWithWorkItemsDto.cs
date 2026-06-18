using Application.Features.Reports.DTOs;
using Application.Features.WorkItems.DTOs;

namespace Application.Features.Sprints.DTOs;

public sealed class SprintWithWorkItemsDto
{
    public required SprintDto Sprint { get; set; }

    public SprintProgressDto SprintProgress { get; set; } = null!;

    public List<WorkItemListItemDto> Epics { get; set; } = new();

    public List<WorkItemListItemDto> OrphanWorkItems { get; set; } = new();

    public List<SprintUserStoryGroupDto> UserStories { get; set; } = new();
}

public sealed class SprintUserStoryGroupDto
{
    public required WorkItemListItemDto UserStory { get; set; }

    public List<WorkItemListItemDto> WorkItems { get; set; } = new();
}