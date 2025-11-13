namespace Domain.Entities;
public class ProjectHistory : Entity
{
    public required string ProjectId { get; set; }
    public required string ChangedByUserId { get; set; }
    public string? PropertyName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    // Navigation
    public Project Project { get; set; } = null!;
    public ApplicationUser ChangedByUser { get; set; } = null!;
}
