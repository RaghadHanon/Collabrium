using Application.Features.Reports.DTOs;

namespace Application.Interfaces.ReadModels;
public interface IReportsReadModel
{
    Task<TenantInfoReport> GenerateTenantInfoReportAsync(string tenantId);
}