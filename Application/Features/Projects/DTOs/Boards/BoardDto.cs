namespace Application.Features.Projects.DTOs.Boards;

public class BoardDto
{
    public required string ProjectId { get; init; }
    public string? SprintId { get; init; }
    public required List<UserStoryBoardGroupDto> UserStoryRows { get; init; }
    public required OrphanBoardGroupDto OrphanRow { get; init; }
}