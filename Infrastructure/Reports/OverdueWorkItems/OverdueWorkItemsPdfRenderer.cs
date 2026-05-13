using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;

namespace Infrastructure.Reports.OverdueWorkItems;

public class OverdueWorkItemsPdfRenderer : IOverdueWorkItemsPdfRenderer
{
    private readonly IOverdueWorkItemsChartRenderer _chartRenderer;
    private readonly IReportBrandingProvider _brandingProvider;

    public OverdueWorkItemsPdfRenderer(
        IOverdueWorkItemsChartRenderer chartRenderer,
        IReportBrandingProvider brandingProvider)
    {
        _chartRenderer = chartRenderer;
        _brandingProvider = brandingProvider;
    }

    public byte[] Render(OverdueWorkItemsReportDto report)
    {
        var priorityChart = _chartRenderer.RenderPriorityChart(report);
        var statusChart = _chartRenderer.RenderStatusChart(report);
        var daysOverdueChart = _chartRenderer.RenderDaysOverdueChart(report);

        var logoBytes = _brandingProvider.GetLogoAsync().GetAwaiter().GetResult();

        var document = new OverdueWorkItemsReportDocument(
            report,
            priorityChart,
            statusChart,
            daysOverdueChart,
            logoBytes);

        return document.GeneratePdf();
    }
}