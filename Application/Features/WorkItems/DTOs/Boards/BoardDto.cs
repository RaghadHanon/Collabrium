namespace Application.Features.WorkItems.DTOs.Boards;

public class BoardDto
{
    public required string ProjectId { get; init; }
    public string? SprintId { get; init; }
    public required List<BoardColumnDto> Columns { get; init; }
}