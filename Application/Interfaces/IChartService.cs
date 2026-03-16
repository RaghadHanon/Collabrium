using Application.Features.Reports.DTOs;

namespace Infrastructure.Pdf;
public interface IChartService
{
    byte[] CreateBarChart(CycleTimeReport data);
    byte[] CreateDistributionChart(CycleTimeReport data);
    byte[] CreateSummaryChart(CycleTimeReport data);
}