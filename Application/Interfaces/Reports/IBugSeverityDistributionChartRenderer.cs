using Application.Features.Reports.DTOs;

namespace Application.Interfaces.Reports;

public interface IBugSeverityDistributionChartRenderer
{
    byte[] RenderMainChart(BugSeverityDistributionReportDto data);
    byte[] RenderPercentageChart(BugSeverityDistributionReportDto data);
    byte[] RenderRiskMixChart(BugSeverityDistributionReportDto data);
}