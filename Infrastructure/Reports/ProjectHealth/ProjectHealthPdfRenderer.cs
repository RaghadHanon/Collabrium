using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;

namespace Infrastructure.Reports.ProjectHealth;

public class ProjectHealthPdfRenderer : IProjectHealthPdfRenderer
{
    private readonly IProjectHealthChartRenderer _chartRenderer;
    private readonly IReportBrandingProvider _brandingProvider;

    public ProjectHealthPdfRenderer(
        IProjectHealthChartRenderer chartRenderer,
        IReportBrandingProvider brandingProvider)
    {
        _chartRenderer = chartRenderer;
        _brandingProvider = brandingProvider;
    }

    public byte[] Render(ProjectHealthReportDto report)
    {
        var healthScoreChart = _chartRenderer.RenderHealthScoreChart(report);
        var healthStatusChart = _chartRenderer.RenderHealthStatusChart(report);
        var riskFactorsChart = _chartRenderer.RenderRiskFactorsChart(report);

        var logoBytes = _brandingProvider.GetLogoAsync().GetAwaiter().GetResult();

        var document = new ProjectHealthReportDocument(
            report,
            healthScoreChart,
            healthStatusChart,
            riskFactorsChart,
            logoBytes);

        return document.GeneratePdf();
    }
}