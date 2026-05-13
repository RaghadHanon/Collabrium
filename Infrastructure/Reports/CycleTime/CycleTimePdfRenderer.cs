using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;

namespace Infrastructure.Reports.CycleTime;

public class CycleTimePdfRenderer : ICycleTimePdfRenderer
{
    private readonly ICycleTimeChartRenderer _chartRenderer;
    private readonly IReportBrandingProvider _brandingProvider;

    public CycleTimePdfRenderer(
        ICycleTimeChartRenderer chartRenderer,
        IReportBrandingProvider brandingProvider)
    {
        _chartRenderer = chartRenderer;
        _brandingProvider = brandingProvider;
    }

    public byte[] Render(CycleTimeReport report)
    {
        var mainChart = _chartRenderer.RenderMainChart(report);
        var distributionChart = _chartRenderer.RenderSecondaryChart(report);
        var summaryChart = _chartRenderer.RenderSummaryChart(report);

        var logoBytes = _brandingProvider.GetLogoAsync().GetAwaiter().GetResult();

        var document = new CycleTimeReportDocument(
            report,
            mainChart,
            distributionChart,
            summaryChart,
            logoBytes);

        return document.GeneratePdf();
    }
}