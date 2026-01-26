namespace Infrastructure.Persistence.Entities;

public class TeamMemberEntity
{
    public string TeamId { get; set; } = null!;
    public TeamEntity Team { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public ApplicationUserEntity User { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
