using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.Charts;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace Infrastructure.Reports.TeamsProductivity;

public class TeamsProductivityChartRenderer : ITeamsProductivityChartRenderer
{
    private const int MaxChartItems = 12;

    [Obsolete]
    public byte[] RenderCompletedEffortChart(TeamsProductivityReportDto data)
    {
        var teams = GetRankedTeams(data)
            .Where(team => team.CompletedEffortPoints > 0 || team.CompletedWorkItemsCount > 0)
            .Take(MaxChartItems)
            .ToList();

        if (teams.Count == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Team Completed Effort",
                "Team",
                "Completed Effort Points",
                1400,
                700);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        var xs = Enumerable.Range(0, teams.Count).Select(i => (double) i).ToArray();
        var ys = teams.Select(team => (double) team.CompletedEffortPoints).ToArray();
        var labels = teams.Select(team => ShortenLabel(team.TeamName)).ToArray();

        var bars = plt.Add.Bars(xs, ys);

        foreach (var bar in bars.Bars)
        {
            bar.FillStyle.Color = BaseChartFactory.Primary();
            bar.LineStyle.Width = 0;
        }

        AddValueLabels(plt, xs, ys);

        BaseChartFactory.ApplyTitle(plt, "Team Completed Effort", 20);
        BaseChartFactory.ApplyAxisLabels(plt, "Team", "Completed Effort Points");

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
    public byte[] RenderCompletionRateChart(TeamsProductivityReportDto data)
    {
        var teams = GetRankedTeams(data)
            .Where(team => team.AssignedWorkItemsCount > 0)
            .Take(MaxChartItems)
            .ToList();

        if (teams.Count == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Team Completion Rate",
                "Team",
                "Completion %",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        var xs = Enumerable.Range(0, teams.Count).Select(i => (double) i).ToArray();
        var ys = teams.Select(team => team.CompletionPercentage).ToArray();
        var labels = teams.Select(team => ShortenLabel(team.TeamName)).ToArray();

        var bars = plt.Add.Bars(xs, ys);

        foreach (var bar in bars.Bars)
        {
            bar.FillStyle.Color = BaseChartFactory.Accent();
            bar.LineStyle.Width = 0;
        }

        AddValueLabels(plt, xs, ys, suffix: "%", valueFormat: "N1");

        BaseChartFactory.ApplyTitle(plt, "Team Completion Rate", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "Team", "Completion %");

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
    public byte[] RenderRiskChart(TeamsProductivityReportDto data)
    {
        var inProgressCount = data.Teams?.Sum(team => team.InProgressWorkItemsCount) ?? 0;

        if (data.TotalBlockedWorkItems == 0 &&
            data.TotalOverdueWorkItems == 0 &&
            inProgressCount == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Team Workload Risk",
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

        AddValueLabels(plt, xs, ys);

        BaseChartFactory.ApplyTitle(plt, "Team Workload Risk", 17);
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

    private static List<TeamProductivityItemDto> GetRankedTeams(TeamsProductivityReportDto data)
    {
        return data.Teams?
            .OrderByDescending(team => team.CompletedEffortPoints)
            .ThenByDescending(team => team.CompletedWorkItemsCount)
            .ThenByDescending(team => team.CompletionPercentage)
            .ToList()
            ?? new List<TeamProductivityItemDto>();
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