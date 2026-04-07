using Application.Features.Reports.DTOs;
using ScottPlot;
using ScottPlot.TickGenerators;
using ScottPlot.Statistics;

namespace Infrastructure.Pdf;

public class ChartService : IChartService
{
    private readonly Color Ink = Color.FromHex("#0F172A");
    private readonly Color Slate = Color.FromHex("#475569");
    private readonly Color Muted = Color.FromHex("#94A3B8");
    private readonly Color Grid = Color.FromHex("#EEF2F7");

    private readonly Color Primary = Color.FromHex("#2563EB");
    private readonly Color PrimarySoft = Color.FromHex("#DBEAFE");
    private readonly Color Accent = Color.FromHex("#14B8A6");
    private readonly Color Warning = Color.FromHex("#F59E0B");
    private readonly Color Danger = Color.FromHex("#EF4444");

    private Plot CreateBasePlot()
    {
        var plt = new Plot();

        plt.FigureBackground.Color = Colors.White;
        plt.DataBackground.Color = Colors.White;

        plt.Grid.MajorLineColor = Grid;
        plt.Grid.MinorLineColor = Grid.WithAlpha(0.25);

        plt.Axes.Left.TickLabelStyle.FontSize = 11;
        plt.Axes.Bottom.TickLabelStyle.FontSize = 11;

        plt.Axes.Left.TickLabelStyle.ForeColor = Slate;
        plt.Axes.Bottom.TickLabelStyle.ForeColor = Slate;

        plt.Axes.Left.Label.ForeColor = Slate;
        plt.Axes.Bottom.Label.ForeColor = Slate;
        plt.Axes.Title.Label.ForeColor = Ink;

        return plt;
    }

    private static double[] GetValidValues(CycleTimeReport data)
    {
        return data.Items?
            .Select(x => (double) x.CycleTimeDays)
            .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
            .ToArray()
            ?? Array.Empty<double>();
    }

    private byte[] CreateEmptyStateChart(string title, string xLabel, string yLabel, int width, int height)
    {
        var plt = CreateBasePlot();

        plt.Axes.Title.Label.Text = title;
        plt.Axes.Title.Label.FontSize = 18;
        plt.Axes.Title.Label.Bold = true;

        plt.Axes.Left.Label.Text = yLabel;
        plt.Axes.Bottom.Label.Text = xLabel;

        var txt = plt.Add.Text("No data available", 0.5, 0.5);
        txt.Alignment = Alignment.MiddleCenter;
        txt.FontSize = 18;
        txt.Bold = true;

        plt.Axes.SetLimits(0, 1, 0, 1);
        plt.Grid.IsVisible = false;

        return plt.GetImageBytes(width, height);
    }

    public byte[] CreateBarChart(CycleTimeReport data)
    {
        if (data.Items == null || data.Items.Count == 0)
        {
            return CreateEmptyStateChart(
                "Cycle Time Ranking",
                "Work Item",
                "Cycle Time (Days)",
                1200,
                620);
        }

        var plt = CreateBasePlot();

        var ordered = data.Items
            .OrderByDescending(x => x.CycleTimeDays)
            .Take(12)
            .ToList();

        double[] xs = Enumerable.Range(0, ordered.Count).Select(i => (double) i).ToArray();
        double[] ys = ordered.Select(x => (double) x.CycleTimeDays).ToArray();
        string[] labels = ordered.Select(x => x.Code).ToArray();

        var bars = plt.Add.Bars(xs, ys);

        foreach (var bar in bars.Bars)
        {
            bar.FillStyle.Color = Primary;
            bar.LineStyle.Width = 0;
        }

        double maxY = ys.Max();
        double offset = Math.Max(0.5, maxY * 0.03);

        for (int i = 0; i < xs.Length; i++)
        {
            var txt = plt.Add.Text($"{ys[i]:N0}d", xs[i], ys[i] + offset);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 10;
            txt.Bold = true;
        }

        var avg = plt.Add.HorizontalLine(data.AverageCycleTimeDays);
        avg.LineColor = Accent;
        avg.LineWidth = 2;
        avg.LinePattern = LinePattern.Dashed;

        plt.Axes.Title.Label.Text = "Cycle Time Ranking";
        plt.Axes.Title.Label.FontSize = 20;
        plt.Axes.Title.Label.Bold = true;

        plt.Axes.Left.Label.Text = "Cycle Time (Days)";
        plt.Axes.Bottom.Label.Text = "Work Item";

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);
        plt.Axes.Bottom.TickLabelStyle.Rotation = 28;

        plt.Axes.Margins(left: 0.05, right: 0.03, top: 0.12, bottom: 0.15);
        plt.Legend.IsVisible = false;

        return plt.GetImageBytes(1200, 620);
    }

    public byte[] CreateDistributionChart(CycleTimeReport data)
    {
        var values = GetValidValues(data);

        if (values.Length == 0)
        {
            return CreateEmptyStateChart(
                "Distribution",
                "Cycle Time (Days)",
                "Items",
                820,
                420);
        }

        var plt = CreateBasePlot();

        plt.Axes.Title.Label.Text = "Distribution";
        plt.Axes.Title.Label.FontSize = 17;
        plt.Axes.Title.Label.Bold = true;

        plt.Axes.Bottom.Label.Text = "Cycle Time (Days)";
        plt.Axes.Left.Label.Text = "Items";

        double min = values.Min();
        double max = values.Max();

        if (min == max)
        {
            var bars = plt.Add.Bars(
                new double[] { min },
                new double[] { values.Length });

            foreach (var bar in bars.Bars)
            {
                bar.FillStyle.Color = Primary;
                bar.LineStyle.Width = 0;
            }

            plt.Axes.Bottom.TickGenerator = new NumericManual(
                new double[] { min },
                new string[] { $"{min:N0}" });

            plt.Axes.Margins(left: 0.08, right: 0.05, top: 0.12, bottom: 0.10);

            return plt.GetImageBytes(820, 420);
        }

        int binCount = Math.Min(10, Math.Max(5, values.Length));
        var hist = Histogram.WithBinCount(binCount, values);
        var histogramBars = plt.Add.Bars(hist.Bins, hist.Counts);

        foreach (var bar in histogramBars.Bars)
        {
            bar.FillStyle.Color = PrimarySoft;
            bar.LineStyle.Color = Primary;
            bar.LineStyle.Width = 1;
        }

        plt.Axes.Margins(left: 0.08, right: 0.05, top: 0.12, bottom: 0.10);

        return plt.GetImageBytes(820, 420);
    }

    public byte[] CreateSummaryChart(CycleTimeReport data)
    {
        if (data.Items == null || data.Items.Count == 0)
        {
            return CreateEmptyStateChart(
                "Summary Metrics",
                "",
                "Days",
                820,
                420);
        }

        var plt = CreateBasePlot();

        double[] xs = { 0, 1, 2, 3 };
        double[] ys =
        {
            data.MinCycleTimeDays,
            data.MedianCycleTimeDays,
            data.AverageCycleTimeDays,
            data.MaxCycleTimeDays
        };

        string[] labels = { "Min", "Median", "Average", "Max" };
        Color[] colors = { Accent, PrimarySoft, Primary, Danger };

        var bars = plt.Add.Bars(xs, ys);

        for (int i = 0; i < bars.Bars.Count; i++)
        {
            bars.Bars[i].FillStyle.Color = colors[i];
            bars.Bars[i].LineStyle.Width = 0;
        }

        double offset = Math.Max(0.4, ys.Max() * 0.04);

        for (int i = 0; i < xs.Length; i++)
        {
            var txt = plt.Add.Text($"{ys[i]:N1}d", xs[i], ys[i] + offset);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 10;
            txt.Bold = true;
        }

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);

        plt.Axes.Title.Label.Text = "Summary Metrics";
        plt.Axes.Title.Label.FontSize = 17;
        plt.Axes.Title.Label.Bold = true;
        plt.Axes.Left.Label.Text = "Days";

        plt.Grid.IsVisible = false;
        plt.Axes.Margins(left: 0.08, right: 0.05, top: 0.14, bottom: 0.10);

        return plt.GetImageBytes(820, 420);
    }
}