using Application.Features.WorkItems.DTOs;

namespace Application.Features.Projects.DTOs.Boards;

public class UserStoryBoardGroupDto
{
    public required WorkItemListItemDto ParentUserStory { get; init; }
    public required List<BoardColumnDto>? Columns { get; init; }
}
