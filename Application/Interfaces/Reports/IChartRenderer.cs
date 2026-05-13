namespace Application.Interfaces.Reports;

public interface IChartRenderer<in TReport>
{
    byte[] RenderMainChart(TReport report);
    byte[] RenderSecondaryChart(TReport report);
    byte[] RenderSummaryChart(TReport report);
}