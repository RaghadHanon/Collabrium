using Application.Features.Reports.DTOs;
using Application.Interfaces.Reports;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.Charts;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace Infrastructure.Reports.TenantInfo;

public class TenantInfoChartRenderer : ITenantInfoChartRenderer
{
    public byte[] RenderMainChart(TenantInfoReport data)
    {
        var totalTeams = data.TeamsSummary.TotalTeamsCount;
        var totalProjects = data.ProjectsSummary.TotalProjectsCount;
        var totalSprints = data.SprintsSummary.TotalSprintsCount;
        var totalUsers = data.UsersSummary.TotalUsersCount;
        var totalWorkItems =
            data.WorkItemsSummary.EpicsCount +
            data.WorkItemsSummary.UserStoriesCount +
            data.WorkItemsSummary.TaskItemCount +
            data.WorkItemsSummary.BugsCount +
            data.WorkItemsSummary.TestCasesCount;

        if (totalTeams == 0 && totalProjects == 0 && totalSprints == 0 && totalUsers == 0 && totalWorkItems == 0)
        {
            return ChartEmptyStateFactory.Create(
                "Tenant Overview",
                "",
                "Count",
                1400,
                700);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        double[] xs = { 0, 1, 2, 3, 4 };
        double[] ys = { totalTeams, totalProjects, totalSprints, totalUsers, totalWorkItems };
        string[] labels = { "Teams", "Projects", "Sprints", "Users", "Work Items" };

        var bars = plt.Add.Bars(xs, ys);

        foreach (var bar in bars.Bars)
        {
            bar.FillStyle.Color = BaseChartFactory.Primary();
            bar.LineStyle.Width = 0;
        }

        var offset = Math.Max(0.4, ys.Max() * 0.04);

        for (int i = 0; i < xs.Length; i++)
        {
            var txt = plt.Add.Text($"{ys[i]:N0}", xs[i], ys[i] + offset);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 10;
            txt.Bold = true;
        }

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);

        BaseChartFactory.ApplyTitle(plt, "Tenant Overview", 20);
        BaseChartFactory.ApplyAxisLabels(plt, "", "Count");

        plt.Grid.IsVisible = false;
        BaseChartFactory.ApplyDefaultMargins(plt, left: 0.08, right: 0.05, top: 0.14, bottom: 0.12);

        return plt.GetImageBytes(1400, 700);
    }

    public byte[] RenderSecondaryChart(TenantInfoReport data)
    {
        var values = new[]
        {
            data.ProjectsSummary.NotStartedProjectsCount,
            data.ProjectsSummary.InProgressProjectsCount,
            data.ProjectsSummary.CompletedProjectsCount,
            data.ProjectsSummary.OnHoldProjectsCount,
            data.ProjectsSummary.CancelledProjectsCount
        };

        if (values.All(x => x == 0))
        {
            return ChartEmptyStateFactory.Create(
                "Projects by Status",
                "",
                "Count",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        double[] xs = { 0, 1, 2, 3, 4 };
        double[] ys = values.Select(x => (double) x).ToArray();
        string[] labels = { "Not Started", "In Progress", "Completed", "On Hold", "Cancelled" };

        ScottPlot.Color[] colors =
        {
            BaseChartFactory.PrimarySoft(),
            BaseChartFactory.Primary(),
            BaseChartFactory.Accent(),
            ScottPlot.Color.FromHex(ReportPalette.Warning),
            BaseChartFactory.Danger()
        };

        var bars = plt.Add.Bars(xs, ys);

        for (int i = 0; i < bars.Bars.Count; i++)
        {
            bars.Bars[i].FillStyle.Color = colors[i];
            bars.Bars[i].LineStyle.Width = 0;
        }

        var offset = Math.Max(0.4, ys.Max() * 0.04);

        for (int i = 0; i < xs.Length; i++)
        {
            var txt = plt.Add.Text($"{ys[i]:N0}", xs[i], ys[i] + offset);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 9;
            txt.Bold = true;
        }

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);
        plt.Axes.Bottom.TickLabelStyle.Rotation = 20;

        BaseChartFactory.ApplyTitle(plt, "Projects by Status", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "", "Count");

        BaseChartFactory.ApplyDefaultMargins(plt, left: 0.08, right: 0.05, top: 0.12, bottom: 0.16);

        return plt.GetImageBytes(900, 450);
    }

    public byte[] RenderSummaryChart(TenantInfoReport data)
    {
        var values = new[]
        {
            data.SprintsSummary.PlannedSprintsCount,
            data.SprintsSummary.ActiveSprintsCount,
            data.SprintsSummary.CompletedSprintsCount,
            data.SprintsSummary.CancelledSprintsCount
        };

        if (values.All(x => x == 0))
        {
            return ChartEmptyStateFactory.Create(
                "Sprints by Status",
                "",
                "Count",
                900,
                450);
        }

        var plt = BaseChartFactory.CreateModernPlot();

        double[] xs = { 0, 1, 2, 3 };
        double[] ys = values.Select(x => (double) x).ToArray();
        string[] labels = { "Planned", "Active", "Completed", "Cancelled" };

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

        var offset = Math.Max(0.4, ys.Max() * 0.04);

        for (int i = 0; i < xs.Length; i++)
        {
            var txt = plt.Add.Text($"{ys[i]:N0}", xs[i], ys[i] + offset);
            txt.Alignment = Alignment.LowerCenter;
            txt.FontSize = 10;
            txt.Bold = true;
        }

        plt.Axes.Bottom.TickGenerator = new NumericManual(xs, labels);

        BaseChartFactory.ApplyTitle(plt, "Sprints by Status", 17);
        BaseChartFactory.ApplyAxisLabels(plt, "", "Count");

        plt.Grid.IsVisible = false;
        BaseChartFactory.ApplyDefaultMargins(plt, left: 0.08, right: 0.05, top: 0.14, bottom: 0.12);

        return plt.GetImageBytes(900, 450);
    }
}