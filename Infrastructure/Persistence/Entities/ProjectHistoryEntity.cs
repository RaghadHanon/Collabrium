using Domain.Enums;
using Domain.Models;

namespace Infrastructure.Persistence.Entities;

public class ProjectHistoryEntity : DbEntity
{
    public string ProjectId { get; set; } = null!;
    public ProjectEntity Project { get; set; } = null!;

    public ActionType ActionType { get; set; }
    public ProjectHistoryScope Scope { get; set; }
    public string RelatedEntityId { get; set; } = null!;
    public string RelatedEntityTitle { get; set; } = null!;

    public string MadeByUserId { get; set; } = null!;
    public ApplicationUserEntity MadeByUser { get; set; } = null!;

    public string? PropertyName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}