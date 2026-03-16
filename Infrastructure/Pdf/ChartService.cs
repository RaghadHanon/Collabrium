using Application.Features.Reports.DTOs;
using ScottPlot;
using ScottPlot.TickGenerators;
using ScottPlot.Statistics;

namespace Infrastructure.Pdf;

public class ChartService : IChartService
{
    private readonly Color Primary = Color.FromHex("#2A3F58");
    private readonly Color Accent = Color.FromHex("#ECBF28");
    private readonly Color Grid = Color.FromHex("#DFDFDF");

    private Plot CreateBasePlot()
    {
        var plt = new Plot();

        plt.FigureBackground.Color = Colors.White;
        plt.DataBackground.Color = Colors.White;

        plt.Grid.MajorLineColor = Grid.WithAlpha(.5);

        plt.Axes.Left.Label.Text = "";
        plt.Axes.Bottom.Label.Text = "";

        plt.Axes.Left.TickLabelStyle.FontSize = 11;
        plt.Axes.Bottom.TickLabelStyle.FontSize = 11;

        return plt;
    }

    public byte[] CreateBarChart(CycleTimeReport data)
    {
        var plt = CreateBasePlot();

        double[] xs = Enumerable.Range(0, data.Items.Count).Select(x => (double) x).ToArray();
        double[] ys = data.Items.Select(x => (double) x.CycleTimeDays).ToArray();
        string[] labels = data.Items.Select(x => x.Code).ToArray();
        var bars = plt.Add.Bars(xs, ys);

        foreach (var bar in bars.Bars)
        {
            bar.FillStyle.Color = Primary;
            bar.LineStyle.Width = 0;
        }

        foreach (var pair in xs.Zip(ys))
        {
            double x = pair.First;
            double y = pair.Second;

            var txt = plt.Add.Text($"{y}d", x, y + 0.4);
            txt.FontSize = 11;
            txt.Bold = true;
            txt.Alignment = Alignment.LowerCenter;
        }

        var avg = plt.Add.HorizontalLine(data.AverageCycleTimeDays);
        avg.LineColor = Accent;
        avg.LineWidth = 2;
        avg.LinePattern = LinePattern.Dashed;
        avg.LegendText = "Average";

        plt.Axes.Title.Label.Text = "Cycle Time by Work Item";
        plt.Axes.Title.Label.FontSize = 22;
        plt.Axes.Title.Label.Bold = true;
        plt.Axes.Title.Label.ForeColor = Primary;

        plt.Axes.Left.Label.Text = "Cycle Time (Days)";
        plt.Axes.Bottom.Label.Text = "Work Item";

        var positions = Enumerable.Range(0, labels.Length).Select(x => (double) x).ToArray();
        plt.Axes.Bottom.TickGenerator = new NumericManual(positions, labels);

        plt.Axes.Bottom.TickLabelStyle.Rotation = 35;

        plt.Legend.IsVisible = true;

        return plt.GetImageBytes(900, 450);
    }

    public byte[] CreateDistributionChart(CycleTimeReport data)
    {
        var plt = CreateBasePlot();

        double[] values = data.Items.Select(x => (double) x.CycleTimeDays).ToArray();

        var hist = Histogram.WithBinCount(10, values);
        var bars = plt.Add.Bars(hist.Bins, hist.Counts);

        foreach (var bar in bars.Bars)
        {
            bar.FillStyle.Color = Accent.WithAlpha(.85);
            bar.LineStyle.Width = 0; // Use LineStyle for the border
        }

        plt.Axes.Title.Label.Text = "Cycle Time Distribution";
        plt.Axes.Title.Label.FontSize = 22;
        plt.Axes.Title.Label.Bold = true;
        plt.Axes.Title.Label.ForeColor = Primary;

        plt.Axes.Left.Label.Text = "Number of Items";
        plt.Axes.Bottom.Label.Text = "Cycle Time (Days)";

        return plt.GetImageBytes(600, 420);
    }

    public byte[] CreateSummaryChart(CycleTimeReport data)
    {
        var plt = CreateBasePlot();

        double[] values =
        {
            data.AverageCycleTimeDays,
            data.MedianCycleTimeDays,
            data.MaxCycleTimeDays
        };

        string[] labels =
        {
            "Average",
            "Median",
            "Maximum"
        };

        double[] xs = { 0, 1, 2 };
        var bars = plt.Add.Bars(xs, values);

        foreach (var bar in bars.Bars)
        {
            bar.FillStyle.Color = ScottPlot.Color.FromHex("#2A3F58");
            bar.LineStyle.Width = 0;
        }

        for (int i = 0; i < xs.Length; i++)
        {
            var txt = plt.Add.Text($"{values[i]:N1} days", xs[i], values[i] + 0.4);
            txt.FontSize = 12;
            txt.Bold = true;
            txt.Alignment = Alignment.LowerCenter;
        }

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);

        plt.Axes.Title.Label.Text = "Statistical Summary";
        plt.Axes.Title.Label.FontSize = 22;
        plt.Axes.Title.Label.Bold = true;

        plt.Axes.Left.Label.Text = "Cycle Time (Days)";

        plt.Grid.IsVisible = false;

        return plt.GetImageBytes(600, 420);
    }
}