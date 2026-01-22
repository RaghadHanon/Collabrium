using Domain.Enums;

namespace API.Contracts.Teams;

public class GetTeamsRequest
{
    public string? TeamLeadName { get; set; }
    public string? NameContains { get; set; }
    public string? DescriptionContains { get; set; }
    public Department? Department { get; set; }
    public bool? IsActive { get; set; }
    public int? CapacityMin { get; set; }
    public int? CapacityMax { get; set; }
    public DateTime? UpdatedAfter { get; set; }
    public DateTime? UpdatedBefore { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public DateTime? Cursor { get; set; }
    public int PageSize { get; set; } = 10;
}
