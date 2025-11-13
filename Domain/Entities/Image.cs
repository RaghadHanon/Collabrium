namespace Domain.Entities;

public class Image : Entity
{
    public required string Url { get; set; }
    public required string PublicId { get; set; }
    public required string ApplicationUserId { get; set; }
    // Navigation
    public ApplicationUser ApplicationUser { get; set; } = null!;
}
