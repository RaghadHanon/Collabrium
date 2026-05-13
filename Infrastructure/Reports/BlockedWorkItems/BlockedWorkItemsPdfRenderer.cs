using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;

namespace Infrastructure.Reports.BlockedWorkItems;

public class BlockedWorkItemsPdfRenderer : IBlockedWorkItemsPdfRenderer
{
    private readonly IBlockedWorkItemsChartRenderer _chartRenderer;
    private readonly IReportBrandingProvider _brandingProvider;

    public BlockedWorkItemsPdfRenderer(
        IBlockedWorkItemsChartRenderer chartRenderer,
        IReportBrandingProvider brandingProvider)
    {
        _chartRenderer = chartRenderer;
        _brandingProvider = brandingProvider;
    }

    public byte[] Render(BlockedWorkItemsReportDto report)
    {
        var priorityChart = _chartRenderer.RenderPriorityChart(report);
        var statusChart = _chartRenderer.RenderStatusChart(report);
        var ageChart = _chartRenderer.RenderAgeChart(report);

        var logoBytes = _brandingProvider.GetLogoAsync().GetAwaiter().GetResult();

        var document = new BlockedWorkItemsReportDocument(
            report,
            priorityChart,
            statusChart,
            ageChart,
            logoBytes);

        return document.GeneratePdf();
    }
}