using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;

namespace Infrastructure.Reports.TeamsProductivity;

public class TeamsProductivityPdfRenderer : ITeamsProductivityPdfRenderer
{
    private readonly ITeamsProductivityChartRenderer _chartRenderer;
    private readonly IReportBrandingProvider _brandingProvider;

    public TeamsProductivityPdfRenderer(
        ITeamsProductivityChartRenderer chartRenderer,
        IReportBrandingProvider brandingProvider)
    {
        _chartRenderer = chartRenderer;
        _brandingProvider = brandingProvider;
    }

    public byte[] Render(TeamsProductivityReportDto report)
    {
        var completedEffortChart = _chartRenderer.RenderCompletedEffortChart(report);
        var completionRateChart = _chartRenderer.RenderCompletionRateChart(report);
        var riskChart = _chartRenderer.RenderRiskChart(report);

        var logoBytes = _brandingProvider.GetLogoAsync().GetAwaiter().GetResult();

        var document = new TeamsProductivityReportDocument(
            report,
            completedEffortChart,
            completionRateChart,
            riskChart,
            logoBytes);

        return document.GeneratePdf();
    }
}