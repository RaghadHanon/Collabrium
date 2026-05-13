using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;

namespace Infrastructure.Reports.Velocity;

public class VelocityPdfRenderer : IVelocityPdfRenderer
{
    private readonly IVelocityChartRenderer _chartRenderer;
    private readonly IReportBrandingProvider _brandingProvider;

    public VelocityPdfRenderer(
        IVelocityChartRenderer chartRenderer,
        IReportBrandingProvider brandingProvider)
    {
        _chartRenderer = chartRenderer;
        _brandingProvider = brandingProvider;
    }

    public byte[] Render(VelocityReportDto report)
    {
        var mainChart = _chartRenderer.RenderMainChart(report);
        var percentageChart = _chartRenderer.RenderSecondaryChart(report);
        var summaryChart = _chartRenderer.RenderSummaryChart(report);

        var logoBytes = _brandingProvider.GetLogoAsync().GetAwaiter().GetResult();

        var document = new VelocityReportDocument(
            report,
            mainChart,
            percentageChart,
            summaryChart,
            logoBytes);

        return document.GeneratePdf();
    }
}