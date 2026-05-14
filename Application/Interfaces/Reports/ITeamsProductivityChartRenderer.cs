using Application.Features.Reports.DTOs;

namespace Application.Interfaces.Reports;

public interface ITeamsProductivityChartRenderer
{
    byte[] RenderCompletedEffortChart(TeamsProductivityReportDto data);
    byte[] RenderCompletionRateChart(TeamsProductivityReportDto data);
    byte[] RenderRiskChart(TeamsProductivityReportDto data);
}