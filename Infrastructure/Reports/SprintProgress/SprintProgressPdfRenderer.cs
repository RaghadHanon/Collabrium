using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;

namespace Infrastructure.Reports.SprintProgress;

public class SprintProgressPdfRenderer : ISprintProgressPdfRenderer
{
    private readonly ISprintProgressChartRenderer _chartRenderer;
    private readonly IReportBrandingProvider _brandingProvider;

    public SprintProgressPdfRenderer(
        ISprintProgressChartRenderer chartRenderer,
        IReportBrandingProvider brandingProvider)
    {
        _chartRenderer = chartRenderer;
        _brandingProvider = brandingProvider;
    }

    public byte[] Render(SprintProgressDto report)
    {
        var mainChart = _chartRenderer.RenderMainChart(report);
        var progressChart = _chartRenderer.RenderSecondaryChart(report);
        var summaryChart = _chartRenderer.RenderSummaryChart(report);

        var logoBytes = _brandingProvider.GetLogoAsync().GetAwaiter().GetResult();

        var document = new SprintProgressReportDocument(
            report,
            mainChart,
            progressChart,
            summaryChart,
            logoBytes);

        return document.GeneratePdf();
    }
}