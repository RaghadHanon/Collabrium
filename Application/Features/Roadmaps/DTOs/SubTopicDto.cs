namespace Application.Features.Roadmaps.DTOs;

public sealed class SubTopicDto
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Order { get; set; }
}
