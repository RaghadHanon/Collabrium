using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;

namespace Infrastructure.Reports.BugSeverityDistribution;

public class BugSeverityDistributionPdfRenderer : IBugSeverityDistributionPdfRenderer
{
    private readonly IBugSeverityDistributionChartRenderer _chartRenderer;
    private readonly IReportBrandingProvider _brandingProvider;

    public BugSeverityDistributionPdfRenderer(
        IBugSeverityDistributionChartRenderer chartRenderer,
        IReportBrandingProvider brandingProvider)
    {
        _chartRenderer = chartRenderer;
        _brandingProvider = brandingProvider;
    }

    public byte[] Render(BugSeverityDistributionReportDto report)
    {
        var mainChart = _chartRenderer.RenderMainChart(report);
        var percentageChart = _chartRenderer.RenderPercentageChart(report);
        var riskMixChart = _chartRenderer.RenderRiskMixChart(report);

        var logoBytes = _brandingProvider.GetLogoAsync().GetAwaiter().GetResult();

        var document = new BugSeverityDistributionReportDocument(
            report,
            mainChart,
            percentageChart,
            riskMixChart,
            logoBytes);

        return document.GeneratePdf();
    }
}