using Application.Features.Reports.DTOs;

namespace Application.Interfaces.Reports;

public interface IDeveloperProductivityChartRenderer
{
    byte[] RenderCompletedEffortChart(DeveloperProductivityReportDto data);
    byte[] RenderCompletionRateChart(DeveloperProductivityReportDto data);
    byte[] RenderRiskChart(DeveloperProductivityReportDto data);
}