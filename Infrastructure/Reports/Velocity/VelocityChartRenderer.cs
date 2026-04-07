using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common.Charts;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace Infrastructure.Reports.Velocity;

public class VelocityChartRenderer : IVelocityChartRenderer
{
    public byte[] RenderMainChart(VelocityReportDto data)
    {
        if (data.SprintVelocities == null || data.SprintVelocities.Count == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Completed Story Points by Sprint",
                "Sprint",
                "Story Points",
                1400,
                700);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        var ordered = data.SprintVelocities.ToList();

        double[] xs = Enumerable.Range(0, ordered.Count).Select(i => (double) i).ToArray();
        double[] completed = ordered.Select(x => (double) x.CompletedStoryPoints).ToArray();
        string[] labels = ordered.Select(x => x.SprintCode).ToArray();

        var bars = plt.Add.Bars(xs, completed);

        foreach (var bar in bars.Bars)
        {
            bar.FillStyle.Color = BaseChartFactory.Primary();
            bar.LineStyle.Width = 0;
        }

        double maxY = completed.Length > 0 ? completed.Max() : 0;
        double offset = Math.Max(0.5, maxY * 0.04);

        for (int i = 0; i < xs.Length; i++)
        {
            var txt = plt.Add.Text($"{completed[i]:N0}", xs[i], completed[i] + offset);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 9;
            txt.Bold = true;
        }

        BaseChartFactory.ApplyTitle(plt, "Completed Story Points by Sprint", 20);
        BaseChartFactory.ApplyAxisLabels(plt, "Sprint", "Story Points");

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);
        plt.Axes.Bottom.TickLabelStyle.Rotation = 25;

        BaseChartFactory.ApplyDefaultMargins(plt, left: 0.06, right: 0.03, top: 0.12, bottom: 0.18);
        plt.Legend.IsVisible = false;

        return plt.GetImageBytes(1400, 700);
    }

    public byte[] RenderSecondaryChart(VelocityReportDto data)
    {
        if (data.SprintVelocities == null || data.SprintVelocities.Count == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Velocity Percentage by Sprint",
                "Sprint",
                "Velocity %",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        var ordered = data.SprintVelocities.ToList();

        double[] xs = Enumerable.Range(0, ordered.Count).Select(i => (double) i).ToArray();
        double[] ys = ordered.Select(x => x.VelocityPercentage).ToArray();
        string[] labels = ordered.Select(x => x.SprintCode).ToArray();

        var bars = plt.Add.Bars(xs, ys);

        foreach (var bar in bars.Bars)
        {
            bar.FillStyle.Color = BaseChartFactory.Accent();
            bar.LineStyle.Width = 0;
        }

        for (int i = 0; i < xs.Length; i++)
        {
            var txt = plt.Add.Text($"{ys[i]:N1}%", xs[i], ys[i] + 2);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 9;
            txt.Bold = true;
        }

        var target = plt.Add.HorizontalLine(100);
        target.LineColor = BaseChartFactory.Danger();
        target.LineWidth = 2;
        target.LinePattern = LinePattern.Dashed;

        BaseChartFactory.ApplyTitle(plt, "Velocity Percentage by Sprint", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "Sprint", "Velocity %");

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);
        plt.Axes.Bottom.TickLabelStyle.Rotation = 25;

        BaseChartFactory.ApplyDefaultMargins(plt, left: 0.08, right: 0.05, top: 0.12, bottom: 0.16);

        return plt.GetImageBytes(900, 450);
    }

    public byte[] RenderSummaryChart(VelocityReportDto data)
    {
        if (data.SprintVelocities == null || data.SprintVelocities.Count == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Total vs Completed Story Points",
                "",
                "Story Points",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        double[] xs = { 0, 1, 2 };
        double[] ys =
        {
            data.TotalStoryPoints,
            data.CompletedStoryPoints,
            data.AvgVelocityPercentage
        };

        string[] labels = { "Total SP", "Completed SP", "Avg Velocity %" };
        ScottPlot.Color[] colors =
        {
            BaseChartFactory.PrimarySoft(),
            BaseChartFactory.Primary(),
            BaseChartFactory.Accent()
        };

        var bars = plt.Add.Bars(xs, ys);

        for (int i = 0; i < bars.Bars.Count; i++)
        {
            bars.Bars[i].FillStyle.Color = colors[i];
            bars.Bars[i].LineStyle.Width = 0;
        }

        double offset = Math.Max(0.4, ys.Max() * 0.04);

        for (int i = 0; i < xs.Length; i++)
        {
            string label = i == 2 ? $"{ys[i]:N1}%" : $"{ys[i]:N0}";
            var txt = plt.Add.Text(label, xs[i], ys[i] + offset);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 10;
            txt.Bold = true;
        }

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);

        BaseChartFactory.ApplyTitle(plt, "Total vs Completed Story Points", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "", "Value");

        plt.Grid.IsVisible = false;
        BaseChartFactory.ApplyDefaultMargins(plt, left: 0.08, right: 0.05, top: 0.14, bottom: 0.12);

        return plt.GetImageBytes(900, 450);
    }
}