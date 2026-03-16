using Domain.Enums;

namespace API.Contracts.Projects;

public class GetProjectsRequest
{
    public string? TeamId { get; set; }
    public string? NameContains { get; set; }
    public bool AssignedToTeam { get; set; }

    public string? Key { get; set; }
    public ProjectStatus? Status { get; set; }

    public DateTime? StartAfter { get; set; }
    public DateTime? StartBefore { get; set; }

    public DateTime? EndAfter { get; set; }
    public DateTime? EndBefore { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    public DateTime? Cursor { get; set; }
    public int PageSize { get; set; } = 10;
}
