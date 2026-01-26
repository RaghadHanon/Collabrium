namespace Infrastructure.Persistence.Entities;

public class ProjectHistoryEntity : DbEntity
{
    public string ProjectId { get; set; } = null!;
    public ProjectEntity Project { get; set; } = null!;

    public string ChangedByUserId { get; set; } = null!;
    public ApplicationUserEntity ChangedByUser { get; set; } = null!;

    public string? PropertyName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}