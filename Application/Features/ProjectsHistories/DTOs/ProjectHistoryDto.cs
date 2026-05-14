using Domain.Enums;

namespace Application.Features.ProjectsHistories.DTOs;

public class ProjectHistoryDto
{
    public string Id { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public ProjectHistoryScope Scope { get; init; }
    public ActionType ActionType { get; init; }

    public string RelatedEntityId { get; init; } = null!;
    public string RelatedEntityCode { get; init; } = null!;
    public string RelatedEntityTitle { get; init; } = null!;

    public string MadeByUserId { get; init; } = null!;
    public string? MadeByUserName { get; init; }

    public string? PropertyName { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
}