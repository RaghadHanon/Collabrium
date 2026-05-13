using Application.Features.Reports.DTOs;

namespace Application.Interfaces.Reports;

public interface IBlockedWorkItemsChartRenderer
{
    byte[] RenderPriorityChart(BlockedWorkItemsReportDto data);
    byte[] RenderStatusChart(BlockedWorkItemsReportDto data);
    byte[] RenderAgeChart(BlockedWorkItemsReportDto data);
}