namespace API.Contracts.Teams;

public class AddMembersToTeamRequest
{
    public required string TeamId { get; set; }
    public required string[] MemberIds { get; set; } = Array.Empty<string>();
}