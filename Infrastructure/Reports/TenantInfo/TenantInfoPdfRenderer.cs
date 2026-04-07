using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;

namespace Infrastructure.Reports.TenantInfo;

public class TenantInfoPdfRenderer : ITenantInfoPdfRenderer
{
    private readonly ITenantInfoChartRenderer _chartRenderer;
    private readonly IReportBrandingProvider _brandingProvider;

    public TenantInfoPdfRenderer(
        ITenantInfoChartRenderer chartRenderer,
        IReportBrandingProvider brandingProvider)
    {
        _chartRenderer = chartRenderer;
        _brandingProvider = brandingProvider;
    }

    public byte[] Render(TenantInfoReport report)
    {
        var mainChart = _chartRenderer.RenderMainChart(report);
        var projectsChart = _chartRenderer.RenderSecondaryChart(report);
        var sprintsChart = _chartRenderer.RenderSummaryChart(report);
        var logoBytes = _brandingProvider.GetLogoAsync().GetAwaiter().GetResult();

        var document = new TenantInfoReportDocument(
            report,
            mainChart,
            projectsChart,
            sprintsChart,
            logoBytes);

        return document.GeneratePdf();
    }
}