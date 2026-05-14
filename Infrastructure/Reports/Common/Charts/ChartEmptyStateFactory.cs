using ScottPlot;

namespace Infrastructure.Reports.Common.Charts;

public static class ChartEmptyStateFactory
{
    [Obsolete]
    public static byte[] Create(
        string title,
        string xLabel,
        string yLabel,
        int width,
        int height)
    {
        var plt = BaseChartFactory.CreateModernPlot();

        BaseChartFactory.ApplyTitle(plt, title);
        BaseChartFactory.ApplyAxisLabels(plt, xLabel, yLabel);

        var txt = plt.Add.Text("No data available", 0.5, 0.5);
        txt.Alignment = Alignment.MiddleCenter;
        txt.FontSize = 18;
        txt.Bold = true;

        plt.Axes.SetLimits(0, 1, 0, 1);
        plt.Grid.IsVisible = false;

        return plt.GetImageBytes(width, height);
    }
}