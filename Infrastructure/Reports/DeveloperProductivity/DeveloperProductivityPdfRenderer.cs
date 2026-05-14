using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;

namespace Infrastructure.Reports.DeveloperProductivity;

public class DeveloperProductivityPdfRenderer : IDeveloperProductivityPdfRenderer
{
    private readonly IDeveloperProductivityChartRenderer _chartRenderer;
    private readonly IReportBrandingProvider _brandingProvider;

    public DeveloperProductivityPdfRenderer(
        IDeveloperProductivityChartRenderer chartRenderer,
        IReportBrandingProvider brandingProvider)
    {
        _chartRenderer = chartRenderer;
        _brandingProvider = brandingProvider;
    }

    public byte[] Render(DeveloperProductivityReportDto report)
    {
        var completedEffortChart = _chartRenderer.RenderCompletedEffortChart(report);
        var completionRateChart = _chartRenderer.RenderCompletionRateChart(report);
        var riskChart = _chartRenderer.RenderRiskChart(report);

        var logoBytes = _brandingProvider.GetLogoAsync().GetAwaiter().GetResult();

        var document = new DeveloperProductivityReportDocument(
            report,
            completedEffortChart,
            completionRateChart,
            riskChart,
            logoBytes);

        return document.GeneratePdf();
    }
}