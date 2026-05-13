using Application.Features.Reports.DTOs;

namespace Application.Interfaces.Reports;

public interface IOverdueWorkItemsChartRenderer
{
    byte[] RenderPriorityChart(OverdueWorkItemsReportDto data);
    byte[] RenderStatusChart(OverdueWorkItemsReportDto data);
    byte[] RenderDaysOverdueChart(OverdueWorkItemsReportDto data);
}
