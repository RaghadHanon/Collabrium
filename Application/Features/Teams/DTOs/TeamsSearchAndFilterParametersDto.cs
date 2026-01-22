using Application.Common.DTOs;
using Domain.Enums;

namespace Application.Features.Teams.DTOs;
public class TeamsSearchAndFilterParametersDto : QueryParametersDto<DateTime?>
{
    public string? NameContains { get; set; }
    public string? DescriptionContains { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? TenantId { get; set; }
    public string? TeamLeadName { get; set; }
    public Department? Department { get; set; }
    public int? CapacityMin { get; set; }
    public int? CapacityMax { get; set; }
    public DateTime? UpdatedAfter { get; set; }
    public DateTime? UpdatedBefore { get; set; }
}
