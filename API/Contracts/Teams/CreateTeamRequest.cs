namespace API.Contracts.Teams;

public class CreateTeamRequest
{
    public required string TeamLeadId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Department { get; set; }
    public int Capacity { get; set; }
}