using Application.Features.WorkItems.DTOs;
using Domain.Enums;

namespace Application.Features.Projects.DTOs.Boards;

public class BoardColumnDto
{
    public int Count { get; set; }
    public required WorkItemStatus Status { get; init; }
    public required string Title { get; init; }
    public required List<WorkItemListItemDto> Items { get; init; }
}