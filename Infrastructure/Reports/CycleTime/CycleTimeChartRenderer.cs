using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common.Charts;
using ScottPlot;
using ScottPlot.Statistics;
using ScottPlot.TickGenerators;

namespace Infrastructure.Reports.CycleTime;

public class CycleTimeChartRenderer : ICycleTimeChartRenderer
{
    [Obsolete]
    public byte[] RenderMainChart(CycleTimeReport data)
    {
        if (data.Items == null || data.Items.Count == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Cycle Time Ranking",
                "Work Item",
                "Cycle Time (Days)",
                1400,
                700);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        var ordered = data.Items
            .OrderByDescending(x => x.CycleTimeDays)
            .ToList();

        var xs = Enumerable.Range(0, ordered.Count).Select(i => (double) i).ToArray();
        var ys = ordered.Select(x => (double) x.CycleTimeDays).ToArray();
        var labels = ordered.Select(x => x.Code).ToArray();

        var bars = plt.Add.Bars(xs, ys);

        foreach (var bar in bars.Bars)
        {
            bar.FillStyle.Color = BaseChartFactory.Primary();
            bar.LineStyle.Width = 0;
        }

        var maxY = ys.Max();
        var offset = Math.Max(0.5, maxY * 0.03);

        for (var i = 0; i < xs.Length; i++)
        {
            var txt = plt.Add.Text($"{ys[i]:N0}d", xs[i], ys[i] + offset);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 9;
            txt.Bold = true;
        }

        var avg = plt.Add.HorizontalLine(data.AverageCycleTimeDays);
        avg.LineColor = BaseChartFactory.Accent();
        avg.LineWidth = 2;
        avg.LinePattern = LinePattern.Dashed;

        BaseChartFactory.ApplyTitle(plt, "Cycle Time Ranking", 20);
        BaseChartFactory.ApplyAxisLabels(plt, "Work Item", "Cycle Time (Days)");

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);
        plt.Axes.Bottom.TickLabelStyle.Rotation = 35;

        BaseChartFactory.ApplyDefaultMargins(plt, left: 0.05, right: 0.03, top: 0.12, bottom: 0.22);
        plt.Legend.IsVisible = false;

        return plt.GetImageBytes(1400, 700);
    }

    [Obsolete]
    public byte[] RenderSecondaryChart(CycleTimeReport data)
    {
        var values = GetValidValues(data);

        if (values.Length == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Distribution",
                "Cycle Time (Days)",
                "Items",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        BaseChartFactory.ApplyTitle(plt, "Distribution", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "Cycle Time (Days)", "Items");

        var min = values.Min();
        var max = values.Max();

        if (min == max)
        {
            var bars = plt.Add.Bars(
                new double[] { min },
                new double[] { values.Length });

            foreach (var bar in bars.Bars)
            {
                bar.FillStyle.Color = BaseChartFactory.Primary();
                bar.LineStyle.Width = 0;
            }

            plt.Axes.Bottom.TickGenerator = new NumericManual(
                new double[] { min },
                new string[] { $"{min:N0}" });

            BaseChartFactory.ApplyDefaultMargins(plt);

            return plt.GetImageBytes(900, 450);
        }

        var binCount = Math.Min(10, Math.Max(5, values.Length));
        var hist = Histogram.WithBinCount(binCount, values);
        var histogramBars = plt.Add.Bars(hist.Bins, hist.Counts);

        foreach (var bar in histogramBars.Bars)
        {
            bar.FillStyle.Color = BaseChartFactory.PrimarySoft();
            bar.LineStyle.Color = BaseChartFactory.Primary();
            bar.LineStyle.Width = 1;
        }

        BaseChartFactory.ApplyDefaultMargins(plt);

        return plt.GetImageBytes(900, 450);
    }

    [Obsolete]
    public byte[] RenderSummaryChart(CycleTimeReport data)
    {
        if (data.Items == null || data.Items.Count == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Summary Metrics",
                "",
                "Days",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        double[] xs = { 0, 1, 2, 3 };
        double[] ys =
        {
            data.MinCycleTimeDays,
            data.MedianCycleTimeDays,
            data.AverageCycleTimeDays,
            data.MaxCycleTimeDays
        };

        string[] labels = { "Min", "Median", "Average", "Max" };
        Color[] colors =
        {
            BaseChartFactory.Accent(),
            BaseChartFactory.PrimarySoft(),
            BaseChartFactory.Primary(),
            BaseChartFactory.Danger()
        };

        var bars = plt.Add.Bars(xs, ys);

        for (var i = 0; i < bars.Bars.Count; i++)
        {
            bars.Bars[i].FillStyle.Color = colors[i];
            bars.Bars[i].LineStyle.Width = 0;
        }

        var offset = Math.Max(0.4, ys.Max() * 0.04);

        for (var i = 0; i < xs.Length; i++)
        {
            var txt = plt.Add.Text($"{ys[i]:N1}d", xs[i], ys[i] + offset);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 10;
            txt.Bold = true;
        }

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);

        BaseChartFactory.ApplyTitle(plt, "Summary Metrics", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "", "Days");

        plt.Grid.IsVisible = false;
        BaseChartFactory.ApplyDefaultMargins(plt, left: 0.08, right: 0.05, top: 0.14, bottom: 0.10);

        return plt.GetImageBytes(900, 450);
    }

    private static double[] GetValidValues(CycleTimeReport data)
    {
        return data.Items?
            .Select(x => (double) x.CycleTimeDays)
            .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
            .ToArray()
            ?? Array.Empty<double>();
    }
}