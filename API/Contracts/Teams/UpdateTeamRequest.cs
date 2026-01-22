using Domain.Enums;

namespace API.Contracts.Teams;

public class UpdateTeamRequest
{
    public string? TeamLeadId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Department { get; set; }
    public int? Capacity { get; set; }
}
