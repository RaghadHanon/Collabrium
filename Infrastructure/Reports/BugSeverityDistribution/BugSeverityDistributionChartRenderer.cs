using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Domain.Enums;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.Charts;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace Infrastructure.Reports.BugSeverityDistribution;

public class BugSeverityDistributionChartRenderer : IBugSeverityDistributionChartRenderer
{
    [Obsolete]
    public byte[] RenderMainChart(BugSeverityDistributionReportDto data)
    {
        var items = GetOrderedItems(data);

        if (items.Count == 0 || data.TotalBugs == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Bug Severity Distribution",
                "Severity",
                "Bugs",
                1400,
                700);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        var xs = Enumerable.Range(0, items.Count).Select(i => (double) i).ToArray();
        var ys = items.Select(item => (double) item.Count).ToArray();
        var labels = items.Select(item => item.Severity.ToString()).ToArray();

        var bars = plt.Add.Bars(xs, ys);

        for (var i = 0; i < bars.Bars.Count; i++)
        {
            bars.Bars[i].FillStyle.Color = GetSeverityColor(items[i].Severity);
            bars.Bars[i].LineStyle.Width = 0;
        }

        AddValueLabels(plt, xs, ys, suffix: "");

        BaseChartFactory.ApplyTitle(plt, "Bug Severity Distribution", 20);
        BaseChartFactory.ApplyAxisLabels(plt, "Severity", "Bugs");

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);
        plt.Grid.IsVisible = false;

        BaseChartFactory.ApplyDefaultMargins(
            plt,
            left: 0.06,
            right: 0.04,
            top: 0.14,
            bottom: 0.10);

        return plt.GetImageBytes(1400, 700);
    }

    [Obsolete]
    public byte[] RenderPercentageChart(BugSeverityDistributionReportDto data)
    {
        var items = GetOrderedItems(data);

        if (items.Count == 0 || data.TotalBugs == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Severity Percentage",
                "Severity",
                "Percentage",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        var xs = Enumerable.Range(0, items.Count).Select(i => (double) i).ToArray();
        var ys = items.Select(item => item.Percentage).ToArray();
        var labels = items.Select(item => item.Severity.ToString()).ToArray();

        var bars = plt.Add.Bars(xs, ys);

        for (var i = 0; i < bars.Bars.Count; i++)
        {
            bars.Bars[i].FillStyle.Color = GetSeverityColor(items[i].Severity);
            bars.Bars[i].LineStyle.Width = 0;
        }

        AddValueLabels(plt, xs, ys, suffix: "%");

        BaseChartFactory.ApplyTitle(plt, "Severity Percentage", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "Severity", "Percentage");

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);
        plt.Axes.SetLimitsY(0, Math.Max(100, ys.Max() * 1.18));
        plt.Grid.IsVisible = false;

        BaseChartFactory.ApplyDefaultMargins(
            plt,
            left: 0.08,
            right: 0.04,
            top: 0.16,
            bottom: 0.12);

        return plt.GetImageBytes(900, 450);
    }

    [Obsolete]
    public byte[] RenderRiskMixChart(BugSeverityDistributionReportDto data)
    {
        if (data.TotalBugs == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Risk Mix",
                "Risk Group",
                "Bugs",
                900,
                450);
        }

        var highRisk = data.CriticalBugsCount + data.HighBugsCount;
        var normalRisk = data.MediumBugsCount + data.LowBugsCount;

        var plt = BaseChartFactory.CreateModernPlot();

        double[] xs = { 0, 1 };
        double[] ys = { highRisk, normalRisk };
        string[] labels = { "High Risk", "Normal Risk" };

        var bars = plt.Add.Bars(xs, ys);

        bars.Bars[0].FillStyle.Color = BaseChartFactory.Danger();
        bars.Bars[0].LineStyle.Width = 0;

        bars.Bars[1].FillStyle.Color = BaseChartFactory.Primary();
        bars.Bars[1].LineStyle.Width = 0;

        AddValueLabels(plt, xs, ys, suffix: "");

        BaseChartFactory.ApplyTitle(plt, "Risk Mix", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "Risk Group", "Bugs");

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

    private static List<BugSeverityDistributionItemDto> GetOrderedItems(BugSeverityDistributionReportDto data)
    {
        return data.Items?
            .OrderBy(item => GetSeverityOrder(item.Severity))
            .ToList()
            ?? new List<BugSeverityDistributionItemDto>();
    }

    private static int GetSeverityOrder(WorkItemPriority priority)
    {
        return priority switch
        {
            WorkItemPriority.Low => 0,
            WorkItemPriority.Medium => 1,
            WorkItemPriority.High => 2,
            WorkItemPriority.Critical => 3,
            _ => 99
        };
    }

    private static Color GetSeverityColor(WorkItemPriority priority)
    {
        return priority switch
        {
            WorkItemPriority.Low => BaseChartFactory.Accent(),
            WorkItemPriority.Medium => BaseChartFactory.Primary(),
            WorkItemPriority.High => Color.FromHex(ReportPalette.Warning),
            WorkItemPriority.Critical => BaseChartFactory.Danger(),
            _ => BaseChartFactory.Slate()
        };
    }

    [Obsolete]
    private static void AddValueLabels(Plot plt, double[] xs, double[] ys, string suffix)
    {
        if (xs.Length == 0 || ys.Length == 0)
            return;

        var maxY = ys.Max();
        var offset = Math.Max(0.5, maxY * 0.04);

        for (var i = 0; i < xs.Length; i++)
        {
            var value = suffix == "%"
                ? $"{ys[i]:N1}{suffix}"
                : $"{ys[i]:N0}{suffix}";

            var txt = plt.Add.Text(value, xs[i], ys[i] + offset);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 10;
            txt.Bold = true;
        }
    }
}