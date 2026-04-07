using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common.Charts;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace Infrastructure.Reports.SprintProgress;

public class SprintProgressChartRenderer : ISprintProgressChartRenderer
{
    public byte[] RenderMainChart(SprintProgressDto data)
    {
        if (data.TotalWorkItems == 0 && data.CompletedWorkItems == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Work Items Overview",
                "",
                "Count",
                1200,
                650);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        double[] xs = { 0, 1 };
        double[] ys = { data.TotalWorkItems, data.CompletedWorkItems };
        string[] labels = { "Total Items", "Completed Items" };
        ScottPlot.Color[] colors =
        {
            BaseChartFactory.PrimarySoft(),
            BaseChartFactory.Primary()
        };

        var bars = plt.Add.Bars(xs, ys);

        for (int i = 0; i < bars.Bars.Count; i++)
        {
            bars.Bars[i].FillStyle.Color = colors[i];
            bars.Bars[i].LineStyle.Width = 0;
        }

        double offset = Math.Max(0.4, ys.Max() * 0.05);

        for (int i = 0; i < xs.Length; i++)
        {
            var txt = plt.Add.Text($"{ys[i]:N0}", xs[i], ys[i] + offset);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 11;
            txt.Bold = true;
        }

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);

        BaseChartFactory.ApplyTitle(plt, "Work Items Overview", 20);
        BaseChartFactory.ApplyAxisLabels(plt, "", "Count");

        plt.Grid.IsVisible = false;
        BaseChartFactory.ApplyDefaultMargins(plt, left: 0.08, right: 0.05, top: 0.14, bottom: 0.12);

        return plt.GetImageBytes(1200, 650);
    }

    public byte[] RenderSecondaryChart(SprintProgressDto data)
    {
        if (data.TotalWorkItems == 0 &&
            data.CompletedWorkItems == 0 &&
            data.GeneralProgressPercentage == 0 &&
            data.UserStoryProgressPercentage == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Progress Percentages",
                "",
                "Percentage",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        double[] xs = { 0, 1 };
        double[] ys =
        {
            data.GeneralProgressPercentage,
            data.UserStoryProgressPercentage
        };

        string[] labels = { "General Progress", "User Story Progress" };
        ScottPlot.Color[] colors =
        {
            BaseChartFactory.Accent(),
            BaseChartFactory.Danger()
        };

        var bars = plt.Add.Bars(xs, ys);

        for (int i = 0; i < bars.Bars.Count; i++)
        {
            bars.Bars[i].FillStyle.Color = colors[i];
            bars.Bars[i].LineStyle.Width = 0;
        }

        for (int i = 0; i < xs.Length; i++)
        {
            var txt = plt.Add.Text($"{ys[i]:N1}%", xs[i], ys[i] + 2);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 10;
            txt.Bold = true;
        }

        var target = plt.Add.HorizontalLine(100);
        target.LineColor = BaseChartFactory.Primary();
        target.LineWidth = 2;
        target.LinePattern = LinePattern.Dashed;

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);

        BaseChartFactory.ApplyTitle(plt, "Progress Percentages", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "", "Percentage");

        BaseChartFactory.ApplyDefaultMargins(plt, left: 0.08, right: 0.05, top: 0.12, bottom: 0.14);

        return plt.GetImageBytes(900, 450);
    }

    public byte[] RenderSummaryChart(SprintProgressDto data)
    {
        if (data.TotalWorkItems == 0 &&
            data.CompletedWorkItems == 0 &&
            data.GeneralProgressPercentage == 0 &&
            data.UserStoryProgressPercentage == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Sprint Summary",
                "",
                "Value",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        double[] xs = { 0, 1, 2, 3 };
        double[] ys =
        {
            data.TotalWorkItems,
            data.CompletedWorkItems,
            data.GeneralProgressPercentage,
            data.UserStoryProgressPercentage
        };

        string[] labels =
        {
            "Total Items",
            "Completed",
            "General %",
            "User Story %"
        };

        ScottPlot.Color[] colors =
        {
            BaseChartFactory.PrimarySoft(),
            BaseChartFactory.Primary(),
            BaseChartFactory.Accent(),
            BaseChartFactory.Danger()
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
            string label = i >= 2 ? $"{ys[i]:N1}%" : $"{ys[i]:N0}";
            var txt = plt.Add.Text(label, xs[i], ys[i] + offset);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 10;
            txt.Bold = true;
        }

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);

        BaseChartFactory.ApplyTitle(plt, "Sprint Summary", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "", "Value");

        plt.Grid.IsVisible = false;
        BaseChartFactory.ApplyDefaultMargins(plt, left: 0.08, right: 0.05, top: 0.14, bottom: 0.14);

        return plt.GetImageBytes(900, 450);
    }
}
