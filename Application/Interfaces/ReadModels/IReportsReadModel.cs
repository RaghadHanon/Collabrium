using Application.Features.Reports.DTOs;

namespace Application.Interfaces.ReadModels;
public interface IReportsReadModel
{
    Task<TenantInfoReport> GenerateTenantInfoReportAsync(string tenantId);
    Task<CycleTimeReport> GenerateCycleTimeReportAsync(string tenantId, string projectId, DateTime from, DateTime to, string? sprintId);
}