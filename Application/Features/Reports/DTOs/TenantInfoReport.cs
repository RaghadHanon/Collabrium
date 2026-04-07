using Application.Features.Reports.DTOs.Summaries;
using Application.Features.Tenants.DTOs;

namespace Application.Features.Reports.DTOs;
public class TenantInfoReport
{
    public string TenantId { get; set; } = default!;
    public required TenantDto Tenant { get; set; }
    public DateTime GeneratedAt { get; set; }
    public TeamsSummary TeamsSummary { get; set; } = new TeamsSummary();
    public ProjectsSummary ProjectsSummary { get; set; } = new ProjectsSummary();
    public SprintsSummary SprintsSummary { get; set; } = new SprintsSummary();
    public WorkItemsSummary WorkItemsSummary { get; set; } = new WorkItemsSummary();
    public UsersSummary UsersSummary { get; set; } = new UsersSummary();
}