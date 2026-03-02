using Application.Common.DTOs;
using Domain.Enums;

namespace Application.Features.Sprints.DTOs;

public sealed class SprintsSearchAndFilterParametersDto : QueryParametersDto<DateTime?>
{
    public string? TenantId { get; set; }
    public string? ProjectId { get; set; }

    public string? Name { get; set; }
    public SprintStatus? Status { get; set; }

    public DateTime? StartAfter { get; set; }
    public DateTime? StartBefore { get; set; }

    public DateTime? EndAfter { get; set; }
    public DateTime? EndBefore { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}
