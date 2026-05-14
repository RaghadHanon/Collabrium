using Application.Features.Reports.DTOs;

namespace Application.Interfaces.Reports;

public interface IProjectHealthChartRenderer
{
    byte[] RenderHealthScoreChart(ProjectHealthReportDto data);
    byte[] RenderHealthStatusChart(ProjectHealthReportDto data);
    byte[] RenderRiskFactorsChart(ProjectHealthReportDto data);
}