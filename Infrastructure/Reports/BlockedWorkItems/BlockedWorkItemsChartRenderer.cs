using Application.Features.Reports.DTOs;
using Application.Features.Reports.DTOs.Shared;
using Application.Interfaces.Reports;
using Domain.Enums;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.Charts;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace Infrastructure.Reports.BlockedWorkItems;

public class BlockedWorkItemsChartRenderer : IBlockedWorkItemsChartRenderer
{
    private const int MaxChartItems = 12;

    [Obsolete]
    public byte[] RenderPriorityChart(BlockedWorkItemsReportDto data)
    {
        var items = data.ByPriority?
            .Where(item => item.Count > 0)
            .OrderBy(item => GetPriorityOrder(item.Priority))
            .ToList() ?? new List<WorkItemPriorityDistributionDto>();

        if (items.Count == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Blocked Items by Priority",
                "Priority",
                "Items",
                1400,
                700);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        var xs = Enumerable.Range(0, items.Count).Select(i => (double) i).ToArray();
        var ys = items.Select(item => (double) item.Count).ToArray();
        var labels = items.Select(item => item.Priority.ToString()).ToArray();

        var bars = plt.Add.Bars(xs, ys);

        for (var i = 0; i < bars.Bars.Count; i++)
        {
            bars.Bars[i].FillStyle.Color = GetPriorityColor(items[i].Priority);
            bars.Bars[i].LineStyle.Width = 0;
        }

        AddValueLabels(plt, xs, ys);

        BaseChartFactory.ApplyTitle(plt, "Blocked Items by Priority", 20);
        BaseChartFactory.ApplyAxisLabels(plt, "Priority", "Items");

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
    public byte[] RenderStatusChart(BlockedWorkItemsReportDto data)
    {
        var items = data.ByStatus?
            .Where(item => item.Count > 0)
            .OrderByDescending(item => item.Count)
            .ToList() ?? new List<WorkItemStatusDistributionDto>();

        if (items.Count == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Blocked Items by Status",
                "Status",
                "Items",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        var xs = Enumerable.Range(0, items.Count).Select(i => (double) i).ToArray();
        var ys = items.Select(item => (double) item.Count).ToArray();
        var labels = items.Select(item => ShortenLabel(item.Status.ToString())).ToArray();

        var bars = plt.Add.Bars(xs, ys);

        foreach (var bar in bars.Bars)
        {
            bar.FillStyle.Color = BaseChartFactory.Primary();
            bar.LineStyle.Width = 0;
        }

        AddValueLabels(plt, xs, ys);

        BaseChartFactory.ApplyTitle(plt, "Blocked Items by Status", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "Status", "Items");

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);
        plt.Axes.Bottom.TickLabelStyle.Rotation = 30;
        plt.Grid.IsVisible = false;

        BaseChartFactory.ApplyDefaultMargins(
            plt,
            left: 0.08,
            right: 0.04,
            top: 0.16,
            bottom: 0.22);

        return plt.GetImageBytes(900, 450);
    }

    [Obsolete]
    public byte[] RenderAgeChart(BlockedWorkItemsReportDto data)
    {
        var items = data.Items?
            .OrderByDescending(item => item.BlockedAgeDays)
            .Take(MaxChartItems)
            .ToList() ?? new List<BlockedWorkItemDto>();

        if (items.Count == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Oldest Blocked Items",
                "Work Item",
                "Blocked Age (Days)",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        var xs = Enumerable.Range(0, items.Count).Select(i => (double) i).ToArray();
        var ys = items.Select(item => (double) item.BlockedAgeDays).ToArray();
        var labels = items.Select(item => item.Code).ToArray();

        var bars = plt.Add.Bars(xs, ys);

        foreach (var bar in bars.Bars)
        {
            bar.FillStyle.Color = BaseChartFactory.Danger();
            bar.LineStyle.Width = 0;
        }

        AddValueLabels(plt, xs, ys, suffix: "d");

        BaseChartFactory.ApplyTitle(plt, "Oldest Blocked Items", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "Work Item", "Blocked Age (Days)");

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);
        plt.Axes.Bottom.TickLabelStyle.Rotation = 35;
        plt.Grid.IsVisible = false;

        BaseChartFactory.ApplyDefaultMargins(
            plt,
            left: 0.08,
            right: 0.04,
            top: 0.16,
            bottom: 0.24);

        return plt.GetImageBytes(900, 450);
    }

    private static int GetPriorityOrder(WorkItemPriority priority)
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

    private static Color GetPriorityColor(WorkItemPriority priority)
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