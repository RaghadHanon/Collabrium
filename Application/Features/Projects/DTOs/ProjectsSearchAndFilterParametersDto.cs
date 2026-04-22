using Application.Common.DTOs;
using Domain.Enums;

namespace Application.Features.Projects.DTOs;

public class ProjectsSearchAndFilterParametersDto : QueryParametersDto<DateTime?>
{

    public string? TeamId { get; set; }
    public string? NameContains { get; set; }
    public string? Key { get; set; }
    public bool AssignedToTeam { get; set; }

    public ProjectStatus? Status { get; set; }

    public DateTime? StartAfter { get; set; }
    public DateTime? StartBefore { get; set; }

    public DateTime? EndAfter { get; set; }
    public DateTime? EndBefore { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}