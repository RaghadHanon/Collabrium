using Application.Features.Reports.DTOs;
using Application.Features.Reports.DTOs.Summaries;

namespace Application.Interfaces.ReadModels;
public interface IReportsReadModel
{
    Task<TenantInfoReport> GenerateTenantInfoReportAsync(string tenantId);
    Task<SprintsSummary> GenerateProjectSprintsSummary(string tenantId, string projectId);
    Task<SprintProgressDto> GenerateSprintProgressReport(string tenantId, string projectId, string sprintId);
    Task<CycleTimeReport> GenerateCycleTimeReportAsync(string tenantId, string projectId, DateTime from, DateTime to, string? sprintId);
    Task<VelocityReportDto> GenerateVelocityReport(string tenantId, string projectId);
    Task<BugSeverityDistributionReportDto> GenerateBugSeverityDistributionReportAsync(
        string tenantId,
        DateTime from,
        DateTime to,
        string? projectId = null);
    Task<DeveloperProductivityReportDto> GenerateDeveloperProductivityReportAsync(
        string tenantId,
        DateTime from,
        DateTime to,
        string? projectId = null,
        string? teamId = null,
        string? userId = null);
    Task<TeamsProductivityReportDto> GenerateTeamsProductivityReportAsync(
        string tenantId,
        DateTime from,
        DateTime to,
        string? projectId = null);
    Task<BlockedWorkItemsReportDto> GenerateBlockedWorkItemsReportAsync(
    string tenantId,
    DateTime from,
    DateTime to,
    string? projectId = null,
    string? teamId = null,
    string? assignedToUserId = null,
    int maxItems = 100);
    Task<OverdueWorkItemsReportDto> GenerateOverdueWorkItemsReportAsync(
        string tenantId,
        DateTime from,
        DateTime to,
        string? projectId = null,
        string? teamId = null,
        string? assignedToUserId = null,
        int maxItems = 100);
    Task<ProjectHealthReportDto> GenerateProjectHealthReportAsync(
        string tenantId,
        DateTime from,
        DateTime to,
        string? projectId = null);
}