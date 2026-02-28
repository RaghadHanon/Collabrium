namespace API.Contracts.Teams;

public class AddMembersToTeamRequest
{
    public required string TeamId { get; set; }
    public required string[] MembersIds { get; set; } = Array.Empty<string>();
}