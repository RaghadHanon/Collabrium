using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.Charts;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace Infrastructure.Reports.ProjectHealth;

public class ProjectHealthChartRenderer : IProjectHealthChartRenderer
{
    private const int MaxChartItems = 12;

    [Obsolete]
    public byte[] RenderHealthScoreChart(ProjectHealthReportDto data)
    {
        var projects = GetRankedProjects(data)
            .Take(MaxChartItems)
            .ToList();

        if (projects.Count == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Project Health Score",
                "Project",
                "Health Score",
                1400,
                700);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        var xs = Enumerable.Range(0, projects.Count).Select(i => (double) i).ToArray();
        var ys = projects.Select(project => project.HealthScore).ToArray();
        var labels = projects.Select(project => ShortenLabel(project.ProjectKey)).ToArray();

        var bars = plt.Add.Bars(xs, ys);

        for (var i = 0; i < bars.Bars.Count; i++)
        {
            bars.Bars[i].FillStyle.Color = GetHealthColor(projects[i].HealthStatus);
            bars.Bars[i].LineStyle.Width = 0;
        }

        AddValueLabels(plt, xs, ys, valueFormat: "N0");

        BaseChartFactory.ApplyTitle(plt, "Project Health Score", 20);
        BaseChartFactory.ApplyAxisLabels(plt, "Project", "Health Score");

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);
        plt.Axes.Bottom.TickLabelStyle.Rotation = 35;
        plt.Axes.SetLimitsY(0, 110);
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
    public byte[] RenderHealthStatusChart(ProjectHealthReportDto data)
    {
        if (data.ProjectsCount == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Health Status Distribution",
                "Status",
                "Projects",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        double[] xs = { 0, 1, 2 };
        double[] ys =
        {
            data.HealthyProjectsCount,
            data.AtRiskProjectsCount,
            data.CriticalProjectsCount
        };

        string[] labels = { "Healthy", "At Risk", "Critical" };

        var bars = plt.Add.Bars(xs, ys);

        bars.Bars[0].FillStyle.Color = BaseChartFactory.Accent();
        bars.Bars[0].LineStyle.Width = 0;

        bars.Bars[1].FillStyle.Color = Color.FromHex(ReportPalette.Warning);
        bars.Bars[1].LineStyle.Width = 0;

        bars.Bars[2].FillStyle.Color = BaseChartFactory.Danger();
        bars.Bars[2].LineStyle.Width = 0;

        AddValueLabels(plt, xs, ys, valueFormat: "N0");

        BaseChartFactory.ApplyTitle(plt, "Health Status Distribution", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "Status", "Projects");

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

    [Obsolete]
    public byte[] RenderRiskFactorsChart(ProjectHealthReportDto data)
    {
        var blocked = data.Projects?.Sum(project => project.BlockedWorkItems) ?? 0;
        var overdue = data.Projects?.Sum(project => project.OverdueWorkItems) ?? 0;
        var criticalBugs = data.Projects?.Sum(project => project.CriticalBugs) ?? 0;
        var highBugs = data.Projects?.Sum(project => project.HighBugs) ?? 0;

        if (blocked == 0 && overdue == 0 && criticalBugs == 0 && highBugs == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Risk Factors",
                "Risk Factor",
                "Count",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        double[] xs = { 0, 1, 2, 3 };
        double[] ys =
        {
            blocked,
            overdue,
            criticalBugs,
            highBugs
        };

        string[] labels = { "Blocked", "Overdue", "Critical Bugs", "High Bugs" };

        var bars = plt.Add.Bars(xs, ys);

        bars.Bars[0].FillStyle.Color = BaseChartFactory.Primary();
        bars.Bars[0].LineStyle.Width = 0;

        bars.Bars[1].FillStyle.Color = Color.FromHex(ReportPalette.Warning);
        bars.Bars[1].LineStyle.Width = 0;

        bars.Bars[2].FillStyle.Color = BaseChartFactory.Danger();
        bars.Bars[2].LineStyle.Width = 0;

        bars.Bars[3].FillStyle.Color = Color.FromHex(ReportPalette.Indigo);
        bars.Bars[3].LineStyle.Width = 0;

        AddValueLabels(plt, xs, ys, valueFormat: "N0");

        BaseChartFactory.ApplyTitle(plt, "Risk Factors", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "Risk Factor", "Count");

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);
        plt.Axes.Bottom.TickLabelStyle.Rotation = 25;
        plt.Grid.IsVisible = false;

        BaseChartFactory.ApplyDefaultMargins(
            plt,
            left: 0.08,
            right: 0.04,
            top: 0.16,
            bottom: 0.20);

        return plt.GetImageBytes(900, 450);
    }

    private static List<ProjectHealthItemDto> GetRankedProjects(ProjectHealthReportDto data)
    {
        return data.Projects?
            .OrderBy(project => project.HealthScore)
            .ThenByDescending(project => project.CriticalBugs)
            .ThenByDescending(project => project.OverdueWorkItems)
            .ToList()
            ?? new List<ProjectHealthItemDto>();
    }

    private static Color GetHealthColor(string healthStatus)
    {
        return healthStatus switch
        {
            "Healthy" => BaseChartFactory.Accent(),
            "At Risk" => Color.FromHex(ReportPalette.Warning),
            "Critical" => BaseChartFactory.Danger(),
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