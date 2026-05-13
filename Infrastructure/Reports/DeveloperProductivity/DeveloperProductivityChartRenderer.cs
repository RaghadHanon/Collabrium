using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.Charts;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace Infrastructure.Reports.DeveloperProductivity;

public class DeveloperProductivityChartRenderer : IDeveloperProductivityChartRenderer
{
    private const int MaxChartItems = 12;

    [Obsolete]
    public byte[] RenderCompletedEffortChart(DeveloperProductivityReportDto data)
    {
        var developers = GetRankedDevelopers(data)
            .Where(dev => dev.CompletedEffortPoints > 0 || dev.CompletedWorkItemsCount > 0)
            .Take(MaxChartItems)
            .ToList();

        if (developers.Count == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Developer Completed Effort",
                "Developer",
                "Completed Effort Points",
                1400,
                700);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        var xs = Enumerable.Range(0, developers.Count).Select(i => (double) i).ToArray();
        var ys = developers.Select(dev => (double) dev.CompletedEffortPoints).ToArray();
        var labels = developers.Select(dev => ShortenLabel(dev.FullName)).ToArray();

        var bars = plt.Add.Bars(xs, ys);

        foreach (var bar in bars.Bars)
        {
            bar.FillStyle.Color = BaseChartFactory.Primary();
            bar.LineStyle.Width = 0;
        }

        AddValueLabels(plt, xs, ys, valueFormat: "N0");

        BaseChartFactory.ApplyTitle(plt, "Developer Completed Effort", 20);
        BaseChartFactory.ApplyAxisLabels(plt, "Developer", "Completed Effort Points");

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);
        plt.Axes.Bottom.TickLabelStyle.Rotation = 35;
        plt.Grid.IsVisible = false;

        BaseChartFactory.ApplyDefaultMargins(
            plt,
            left: 0.06,
            right: 0.04,
            top: 0.14,
            bottom: 0.24);

        return plt.GetImageBytes(1400, 700);
    }

    [Obsolete]
    public byte[] RenderCompletionRateChart(DeveloperProductivityReportDto data)
    {
        var developers = GetRankedDevelopers(data)
            .Where(dev => dev.AssignedWorkItemsCount > 0)
            .Take(MaxChartItems)
            .ToList();

        if (developers.Count == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Developer Completion Rate",
                "Developer",
                "Completion %",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        var xs = Enumerable.Range(0, developers.Count).Select(i => (double) i).ToArray();
        var ys = developers.Select(dev => dev.CompletionPercentage).ToArray();
        var labels = developers.Select(dev => ShortenLabel(dev.FullName)).ToArray();

        var bars = plt.Add.Bars(xs, ys);

        foreach (var bar in bars.Bars)
        {
            bar.FillStyle.Color = BaseChartFactory.Accent();
            bar.LineStyle.Width = 0;
        }

        AddValueLabels(plt, xs, ys, suffix: "%", valueFormat: "N1");

        BaseChartFactory.ApplyTitle(plt, "Developer Completion Rate", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "Developer", "Completion %");

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);
        plt.Axes.Bottom.TickLabelStyle.Rotation = 35;
        plt.Axes.SetLimitsY(0, Math.Max(100, ys.Max() * 1.15));
        plt.Grid.IsVisible = false;

        BaseChartFactory.ApplyDefaultMargins(
            plt,
            left: 0.08,
            right: 0.04,
            top: 0.16,
            bottom: 0.26);

        return plt.GetImageBytes(900, 450);
    }

    [Obsolete]
    public byte[] RenderRiskChart(DeveloperProductivityReportDto data)
    {
        var inProgressCount = data.Developers?.Sum(dev => dev.InProgressWorkItemsCount) ?? 0;

        if (data.TotalBlockedWorkItems == 0 &&
            data.TotalOverdueWorkItems == 0 &&
            inProgressCount == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Developer Workload Risk",
                "Risk Type",
                "Items",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        double[] xs = { 0, 1, 2 };
        double[] ys =
        {
            data.TotalBlockedWorkItems,
            data.TotalOverdueWorkItems,
            inProgressCount
        };

        string[] labels = { "Blocked", "Overdue", "In Progress" };

        var bars = plt.Add.Bars(xs, ys);

        bars.Bars[0].FillStyle.Color = BaseChartFactory.Danger();
        bars.Bars[0].LineStyle.Width = 0;

        bars.Bars[1].FillStyle.Color = Color.FromHex(ReportPalette.Warning);
        bars.Bars[1].LineStyle.Width = 0;

        bars.Bars[2].FillStyle.Color = BaseChartFactory.Primary();
        bars.Bars[2].LineStyle.Width = 0;

        AddValueLabels(plt, xs, ys, valueFormat: "N0");

        BaseChartFactory.ApplyTitle(plt, "Developer Workload Risk", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "Risk Type", "Items");

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);
        plt.Grid.IsVisible = false;

        BaseChartFactory.ApplyDefaultMargins(
            plt,
            left: 0.08,
            right: 0.04,
            top: 0.16,
            bottom: 0.12);

        return plt.GetImageBytes(900, 450);
    }

    private static List<DeveloperProductivityItemDto> GetRankedDevelopers(DeveloperProductivityReportDto data)
    {
        return data.Developers?
            .OrderByDescending(dev => dev.CompletedEffortPoints)
            .ThenByDescending(dev => dev.CompletedWorkItemsCount)
            .ThenByDescending(dev => dev.CompletionPercentage)
            .ToList()
            ?? new List<DeveloperProductivityItemDto>();
    }

    [Obsolete]
    private static void AddValueLabels(
        Plot plt,
        double[] xs,
        double[] ys,
        string suffix = "",
        string valueFormat = "N0")
    {
        if (xs.Length == 0 || ys.Length == 0)
            return;

        var maxY = ys.Max();
        var offset = Math.Max(0.5, maxY * 0.04);

        for (var i = 0; i < xs.Length; i++)
        {
            var txt = plt.Add.Text($"{ys[i].ToString(valueFormat)}{suffix}", xs[i], ys[i] + offset);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 9;
            txt.Bold = true;
        }
    }

    private static string ShortenLabel(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "-"
            : value.Length <= 14
            ? value
            : value[..14] + "…";
    }
}