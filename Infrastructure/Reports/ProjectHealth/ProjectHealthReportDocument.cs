using Application.Features.Reports.DTOs;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.PdfComponents;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.ProjectHealth;

public class ProjectHealthReportDocument : IDocument
{
    private const int MaxTableRows = 14;

    private readonly ProjectHealthReportDto _data;
    private readonly byte[] _healthScoreChart;
    private readonly byte[] _healthStatusChart;
    private readonly byte[] _riskFactorsChart;
    private readonly byte[]? _logoBytes;

    public ProjectHealthReportDocument(
        ProjectHealthReportDto data,
        byte[] healthScoreChart,
        byte[] healthStatusChart,
        byte[] riskFactorsChart,
        byte[]? logoBytes = null)
    {
        _data = data;
        _healthScoreChart = healthScoreChart;
        _healthStatusChart = healthStatusChart;
        _riskFactorsChart = riskFactorsChart;
        _logoBytes = logoBytes;
    }

    public DocumentMetadata GetMetadata()
    {
        return DocumentMetadata.Default;
    }

    public void Compose(IDocumentContainer container)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(24);
            page.PageColor(ReportPalette.White);

            page.DefaultTextStyle(ReportTextStyles.Base);

            page.Header().Element(BuildHeader);
            page.Content().PaddingTop(18).Element(BuildContent);
            page.Footer().Element(BuildFooter);
        });
    }

    private void BuildHeader(IContainer container)
    {
        var scope = _data.ProjectId is not null
            ? $"Project {_data.ProjectId}"
            : "All Projects";

        ReportLayoutHelpers.BuildStandardHeader(
            container,
            title: "Project Health Report",
            subtitle: $"Portfolio health analytics • Period: {_data.From:dd MMM yyyy} → {_data.To:dd MMM yyyy}",
            rightTop: scope,
            rightBottom: $"Generated: {_data.GeneratedAt:dd MMM yyyy}",
            logoBytes: _logoBytes);
    }

    private void BuildContent(IContainer container)
    {
        container.Column(col =>
        {
            col.Spacing(16);

            col.Item().Row(row =>
            {
                row.Spacing(16);
                row.RelativeItem(8).Element(BuildHeroCard);
                row.RelativeItem(4).Element(BuildInsightsCard);
            });

            col.Item().Element(BuildKpiRow);

            col.Item().Component(new SectionCard(
                title: "Project Health Score",
                subtitle: "Projects ranked from lowest health score to highest health score",
                image: _healthScoreChart));

            col.Item().Row(row =>
            {
                row.Spacing(16);

                row.RelativeItem().Component(new SectionCard(
                    title: "Health Status Distribution",
                    subtitle: "Healthy, At Risk, and Critical project counts",
                    image: _healthStatusChart));

                row.RelativeItem().Component(new SectionCard(
                    title: "Risk Factors",
                    subtitle: "Blocked work, overdue work, and open high-severity bugs",
                    image: _riskFactorsChart));
            });

            col.Item().Row(row =>
            {
                row.Spacing(16);

                row.RelativeItem(5).Element(BuildRiskBreakdownCard);
                row.RelativeItem(7).Element(BuildTableSection);
            });
        });
    }

    private void BuildHeroCard(IContainer container)
    {
        container.ApplySoftCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Portfolio Health Overview")
                .Style(ReportTextStyles.CardTitle)
                .FontColor(ReportPalette.Primary);

            col.Item().Text($"{_data.AverageHealthScore:N1}/100")
                .FontSize(30)
                .Bold()
                .FontColor(GetAverageHealthColor());

            col.Item().Text(
                    $"From {_data.From:dd MMM yyyy} to {_data.To:dd MMM yyyy}, the report evaluated {_data.ProjectsCount:N0} projects. " +
                    $"{_data.HealthyProjectsCount:N0} projects are healthy, {_data.AtRiskProjectsCount:N0} are at risk, and {_data.CriticalProjectsCount:N0} are critical. " +
                    $"The average health score is {_data.AverageHealthScore:N1}.")
                .Style(ReportTextStyles.Body);

            col.Item().Row(row =>
            {
                row.Spacing(8);

                row.AutoItem().Component(new PillBadge(
                    "Health Score",
                    ReportPalette.PrimarySoft,
                    ReportPalette.Primary));

                row.AutoItem().Component(new PillBadge(
                    "Risk Factors",
                    ReportPalette.WarningSoft,
                    ReportPalette.Warning));

                row.AutoItem().Component(new PillBadge(
                    GetOverallHealthLabel(),
                    GetOverallHealthSoftColor(),
                    GetOverallHealthAccentColor()));
            });
        });
    }

    private void BuildInsightsCard(IContainer container)
    {
        var riskiestProject = GetRankedProjects().FirstOrDefault();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Highlights")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Component(new InsightRow("Period", $"{_data.From:dd MMM} - {_data.To:dd MMM yyyy}"));
            col.Item().Component(new InsightRow("Scope", _data.ProjectId ?? "All Projects"));
            col.Item().Component(new InsightRow("Projects", $"{_data.ProjectsCount:N0}"));
            col.Item().Component(new InsightRow("Average Score", $"{_data.AverageHealthScore:N1}/100"));
            col.Item().Component(new InsightRow("At Risk", $"{_data.AtRiskProjectsCount:N0}"));
            col.Item().Component(new InsightRow("Critical", $"{_data.CriticalProjectsCount:N0}"));

            if (riskiestProject is not null)
            {
                col.Item().PaddingTop(4).Text("Highest attention needed")
                    .Style(ReportTextStyles.Caption)
                    .SemiBold()
                    .FontColor(ReportPalette.Slate);

                col.Item().Text($"{riskiestProject.ProjectKey} • {riskiestProject.HealthScore:N1}/100 • {riskiestProject.HealthStatus}")
                    .Style(ReportTextStyles.Label);
            }
        });
    }

    private void BuildKpiRow(IContainer container)
    {
        container.Row(row =>
        {
            row.Spacing(12);

            row.RelativeItem().Component(new StatCard(
                "Projects",
                $"{_data.ProjectsCount:N0}",
                ReportPalette.Primary,
                ReportPalette.PrimarySoft));

            row.RelativeItem().Component(new StatCard(
                "Avg Health",
                $"{_data.AverageHealthScore:N1}",
                GetAverageHealthColor(),
                GetAverageHealthSoftColor()));

            row.RelativeItem().Component(new StatCard(
                "Healthy",
                $"{_data.HealthyProjectsCount:N0}",
                ReportPalette.Accent,
                ReportPalette.AccentSoft));

            row.RelativeItem().Component(new StatCard(
                "At Risk",
                $"{_data.AtRiskProjectsCount:N0}",
                ReportPalette.Warning,
                ReportPalette.WarningSoft));

            row.RelativeItem().Component(new StatCard(
                "Critical",
                $"{_data.CriticalProjectsCount:N0}",
                ReportPalette.Danger,
                ReportPalette.DangerSoft));
        });
    }

    private void BuildRiskBreakdownCard(IContainer container)
    {
        var blocked = _data.Projects?.Sum(project => project.BlockedWorkItems) ?? 0;
        var overdue = _data.Projects?.Sum(project => project.OverdueWorkItems) ?? 0;
        var criticalBugs = _data.Projects?.Sum(project => project.CriticalBugs) ?? 0;
        var highBugs = _data.Projects?.Sum(project => project.HighBugs) ?? 0;

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Portfolio Risk Breakdown")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Component(new InsightRow("Blocked Work", $"{blocked:N0} items"));
            col.Item().Component(new InsightRow("Overdue Work", $"{overdue:N0} items"));
            col.Item().Component(new InsightRow("Critical Bugs", $"{criticalBugs:N0} open bugs"));
            col.Item().Component(new InsightRow("High Bugs", $"{highBugs:N0} open bugs"));

            col.Item().PaddingTop(6).Text(GetRiskNarrative(blocked, overdue, criticalBugs, highBugs))
                .Style(ReportTextStyles.Body);
        });
    }

    private void BuildTableSection(IContainer container)
    {
        var projects = GetRankedProjects()
            .Take(MaxTableRows)
            .ToList();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Project Health Details")
                .Style(ReportTextStyles.CardTitle);

            if (projects.Count == 0)
            {
                col.Item().Text("No project health data available")
                    .Style(ReportTextStyles.Caption);
                return;
            }

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.4f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.3f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                });

                table.Header(header =>
                {
                    header.Cell().Background(ReportPalette.Soft).Padding(8).Text("Project").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).Text("Status").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Score").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).Text("Health").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Complete").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Blocked").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Overdue").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Bugs").Style(ReportTextStyles.TableHeader);
                });

                for (var i = 0; i < projects.Count; i++)
                {
                    var project = projects[i];
                    var bg = i % 2 == 0 ? ReportPalette.White : ReportPalette.Soft;
                    var bugRisk = project.CriticalBugs + project.HighBugs;

                    table.Cell().Background(bg).Padding(8).Column(inner =>
                    {
                        inner.Item().Text(project.ProjectKey).SemiBold().FontColor(ReportPalette.Ink);
                        inner.Item().Text(Shorten(project.ProjectName, 24)).Style(ReportTextStyles.Caption);
                    });

                    table.Cell().Background(bg).Padding(8).Text(project.ProjectStatus.ToString()).FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{project.HealthScore:N1}").SemiBold().FontColor(GetHealthTextColor(project.HealthStatus));
                    table.Cell().Background(bg).Padding(8).Text(project.HealthStatus).SemiBold().FontColor(GetHealthTextColor(project.HealthStatus));
                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{project.CompletionPercentage:N1}%").FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{project.BlockedWorkItems:N0}").FontColor(GetRiskNumberColor(project.BlockedWorkItems));
                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{project.OverdueWorkItems:N0}").FontColor(GetRiskNumberColor(project.OverdueWorkItems));
                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{bugRisk:N0}").FontColor(GetRiskNumberColor(bugRisk));
                }
            });
        });
    }

    private void BuildFooter(IContainer container)
    {
        ReportLayoutHelpers.BuildStandardFooter(container, "Generated by Collabrium Platform");
    }

    private List<ProjectHealthItemDto> GetRankedProjects()
    {
        return _data.Projects?
            .OrderBy(project => project.HealthScore)
            .ThenByDescending(project => project.CriticalBugs)
            .ThenByDescending(project => project.OverdueWorkItems)
            .ToList()
            ?? new List<ProjectHealthItemDto>();
    }

    private string GetOverallHealthLabel()
    {
        return _data.AverageHealthScore >= 80 ? "Healthy" : _data.AverageHealthScore >= 60 ? "At Risk" : "Critical";
    }

    private string GetAverageHealthColor()
    {
        return _data.AverageHealthScore >= 80
            ? ReportPalette.Accent
            : _data.AverageHealthScore >= 60 ? ReportPalette.Warning : ReportPalette.Danger;
    }

    private string GetAverageHealthSoftColor()
    {
        return _data.AverageHealthScore >= 80
            ? ReportPalette.AccentSoft
            : _data.AverageHealthScore >= 60 ? ReportPalette.WarningSoft : ReportPalette.DangerSoft;
    }

    private string GetOverallHealthSoftColor()
    {
        return GetAverageHealthSoftColor();
    }

    private string GetOverallHealthAccentColor()
    {
        return GetAverageHealthColor();
    }

    private static string GetHealthTextColor(string healthStatus)
    {
        return healthStatus switch
        {
            "Healthy" => ReportPalette.Accent,
            "At Risk" => ReportPalette.Warning,
            "Critical" => ReportPalette.Danger,
            _ => ReportPalette.Slate
        };
    }

    private static string GetRiskNumberColor(int value)
    {
        return value == 0 ? ReportPalette.Accent : value >= 5 ? ReportPalette.Danger : ReportPalette.Warning;
    }

    private string GetRiskNarrative(int blocked, int overdue, int criticalBugs, int highBugs)
    {
        if (_data.ProjectsCount == 0)
            return "No projects were available for this report period.";

        if (_data.CriticalProjectsCount > 0)
            return "Critical projects exist in the portfolio. Review blocked work, overdue items, and high-severity bugs immediately.";

        return _data.AtRiskProjectsCount > 0
            ? "Some projects are at risk. Review delivery progress and risk factors before they become critical."
            : blocked == 0 && overdue == 0 && criticalBugs == 0 && highBugs == 0
            ? "No major portfolio risk factors were detected during this period."
            : "The portfolio is generally healthy, but risk indicators should continue to be monitored.";
    }

    private static string Shorten(string value, int maxLength)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "-"
            : value.Length <= maxLength
            ? value
            : value[..maxLength] + "…";
    }
}