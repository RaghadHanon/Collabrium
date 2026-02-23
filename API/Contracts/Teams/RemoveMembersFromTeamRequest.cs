namespace API.Contracts.Teams;

public class RemoveMembersFromTeamRequest
{
    public required string TeamId { get; set; }
    public required string[] MembersIds { get; set; } = Array.Empty<string>();
}
