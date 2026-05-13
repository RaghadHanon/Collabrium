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
}