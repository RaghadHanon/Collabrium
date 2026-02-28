using Domain.Enums;

namespace Application.Features.WorkItems.DTOs.Boards;

public class BoardColumnDto
{
    public required WorkItemStatus Status { get; init; }
    public required string Title { get; init; }
    public required List<WorkItemListItemDto> Items { get; init; }
}
