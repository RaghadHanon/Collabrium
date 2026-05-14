using Application.Features.Reports.DTOs;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.PdfComponents;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.TeamsProductivity;

public class TeamsProductivityReportDocument : IDocument
{
    private const int MaxTableRows = 14;

    private readonly TeamsProductivityReportDto _data;
    private readonly byte[] _completedEffortChart;
    private readonly byte[] _completionRateChart;
    private readonly byte[] _riskChart;
    private readonly byte[]? _logoBytes;

    public TeamsProductivityReportDocument(
        TeamsProductivityReportDto data,
        byte[] completedEffortChart,
        byte[] completionRateChart,
        byte[] riskChart,
        byte[]? logoBytes = null)
    {
        _data = data;
        _completedEffortChart = completedEffortChart;
        _completionRateChart = completionRateChart;
        _riskChart = riskChart;
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
            title: "Teams Productivity Report",
            subtitle: $"Team delivery analytics • Period: {_data.From:dd MMM yyyy} → {_data.To:dd MMM yyyy}",
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
                title: "Team Completed Effort",
                subtitle: "Teams ranked by completed effort points",
                image: _completedEffortChart));

            col.Item().Row(row =>
            {
                row.Spacing(16);

                row.RelativeItem().Component(new SectionCard(
                    title: "Completion Rate",
                    subtitle: "Completed work items compared with assigned work items",
                    image: _completionRateChart));

                row.RelativeItem().Component(new SectionCard(
                    title: "Workload Risk",
                    subtitle: "Blocked, overdue, and in-progress workload indicators",
                    image: _riskChart));
            });

            col.Item().Row(row =>
            {
                row.Spacing(16);

                row.RelativeItem(5).Element(BuildTopTeamsCard);
                row.RelativeItem(7).Element(BuildTableSection);
            });
        });
    }

    private void BuildHeroCard(IContainer container)
    {
        container.ApplySoftCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Team Productivity Overview")
                .Style(ReportTextStyles.CardTitle)
                .FontColor(ReportPalette.Primary);

            col.Item().Text($"{_data.TotalCompletedEffortPoints:N0} effort points")
                .FontSize(30)
                .Bold()
                .FontColor(ReportPalette.Ink);

            col.Item().Text(
                    $"From {_data.From:dd MMM yyyy} to {_data.To:dd MMM yyyy}, {_data.TeamsCount:N0} teams contributed to delivery. " +
                    $"They completed {_data.TotalCompletedWorkItems:N0} work items with an average completion rate of {_data.AverageCompletionPercentage:N1}%. " +
                    $"Average cycle time is {_data.AverageCycleTimeDays:N1} days.")
                .Style(ReportTextStyles.Body);

            col.Item().Row(row =>
            {
                row.Spacing(8);

                row.AutoItem().Component(new PillBadge(
                    "Team Delivery",
                    ReportPalette.PrimarySoft,
                    ReportPalette.Primary));

                row.AutoItem().Component(new PillBadge(
                    "Effort",
                    ReportPalette.AccentSoft,
                    ReportPalette.Accent));

                row.AutoItem().Component(new PillBadge(
                    GetRiskLabel(),
                    GetRiskSoftColor(),
                    GetRiskAccentColor()));
            });
        });
    }

    private void BuildInsightsCard(IContainer container)
    {
        var topTeam = GetRankedTeams().FirstOrDefault();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Highlights")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Component(new InsightRow("Period", $"{_data.From:dd MMM} - {_data.To:dd MMM yyyy}"));
            col.Item().Component(new InsightRow("Scope", _data.ProjectId ?? "All Projects"));
            col.Item().Component(new InsightRow("Teams", $"{_data.TeamsCount:N0}"));
            col.Item().Component(new InsightRow("Avg Completion", $"{_data.AverageCompletionPercentage:N1}%"));
            col.Item().Component(new InsightRow("Avg Cycle Time", $"{_data.AverageCycleTimeDays:N1} days"));

            if (topTeam is not null)
            {
                col.Item().PaddingTop(4).Text("Top team")
                    .Style(ReportTextStyles.Caption)
                    .SemiBold()
                    .FontColor(ReportPalette.Slate);

                col.Item().Text($"{topTeam.TeamName} • {topTeam.CompletedEffortPoints:N0} effort pts")
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
                "Teams",
                $"{_data.TeamsCount:N0}",
                ReportPalette.Primary,
                ReportPalette.PrimarySoft));

            row.RelativeItem().Component(new StatCard(
                "Completed",
                $"{_data.TotalCompletedWorkItems:N0}",
                ReportPalette.Accent,
                ReportPalette.AccentSoft));

            row.RelativeItem().Component(new StatCard(
                "Effort",
                $"{_data.TotalCompletedEffortPoints:N0}",
                ReportPalette.Indigo,
                ReportPalette.IndigoSoft));

            row.RelativeItem().Component(new StatCard(
                "Blocked",
                $"{_data.TotalBlockedWorkItems:N0}",
                ReportPalette.Danger,
                ReportPalette.DangerSoft));

            row.RelativeItem().Component(new StatCard(
                "Overdue",
                $"{_data.TotalOverdueWorkItems:N0}",
                ReportPalette.Warning,
                ReportPalette.WarningSoft));
        });
    }

    private void BuildTopTeamsCard(IContainer container)
    {
        var topTeams = GetRankedTeams()
            .Take(5)
            .ToList();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Top Teams")
                .Style(ReportTextStyles.CardTitle);

            if (topTeams.Count == 0)
            {
                col.Item().Text("No productivity data available")
                    .Style(ReportTextStyles.Caption);
                return;
            }

            foreach (var team in topTeams)
            {
                col.Item().Element(c =>
                {
                    c.BorderBottom(1)
                        .BorderColor(ReportPalette.Border)
                        .PaddingBottom(8)
                        .PaddingTop(2)
                        .Row(row =>
                        {
                            row.RelativeItem().Column(inner =>
                            {
                                inner.Item().Text(team.TeamName)
                                    .FontSize(10.5f)
                                    .SemiBold()
                                    .FontColor(ReportPalette.Ink);

                                inner.Item().Text($"{team.Department} • Lead: {team.TeamLeadName}")
                                    .Style(ReportTextStyles.Caption);
                            });

                            row.AutoItem().AlignMiddle().Column(inner =>
                            {
                                inner.Item().AlignRight().Text($"{team.CompletedEffortPoints:N0} pts")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(ReportPalette.Primary);

                                inner.Item().AlignRight().Text($"{team.CompletionPercentage:N1}%")
                                    .Style(ReportTextStyles.Caption);
                            });
                        });
                });
            }
        });
    }

    private void BuildTableSection(IContainer container)
    {
        var teams = GetRankedTeams()
            .Take(MaxTableRows)
            .ToList();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Team Details")
                .Style(ReportTextStyles.CardTitle);

            if (teams.Count == 0)
            {
                col.Item().Text("No team data available")
                    .Style(ReportTextStyles.Caption);
                return;
            }

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2.4f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.1f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.1f);
                });

                table.Header(header =>
                {
                    header.Cell().Background(ReportPalette.Soft).Padding(8).Text("Team").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Members").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Projects").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Done").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Effort").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Completion").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Risk").Style(ReportTextStyles.TableHeader);
                });

                for (var i = 0; i < teams.Count; i++)
                {
                    var team = teams[i];
                    var bg = i % 2 == 0 ? ReportPalette.White : ReportPalette.Soft;
                    var riskCount = team.BlockedWorkItemsCount + team.OverdueWorkItemsCount;

                    table.Cell().Background(bg).Padding(8).Column(inner =>
                    {
                        inner.Item().Text(team.TeamName).SemiBold().FontColor(ReportPalette.Ink);
                        inner.Item().Text($"Lead: {team.TeamLeadName}").Style(ReportTextStyles.Caption);
                    });

                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{team.MembersCount:N0}").FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{team.ActiveProjectsCount:N0}").FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{team.CompletedWorkItemsCount:N0}/{team.AssignedWorkItemsCount:N0}").FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{team.CompletedEffortPoints:N0}").SemiBold().FontColor(ReportPalette.Ink);
                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{team.CompletionPercentage:N1}%").FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{riskCount:N0}").SemiBold().FontColor(GetRiskColor(riskCount));
                }
            });
        });
    }

    private void BuildFooter(IContainer container)
    {
        ReportLayoutHelpers.BuildStandardFooter(container, "Generated by Collabrium Platform");
    }

    private List<TeamProductivityItemDto> GetRankedTeams()
    {
        return _data.Teams?
            .OrderByDescending(team => team.CompletedEffortPoints)
            .ThenByDescending(team => team.CompletedWorkItemsCount)
            .ThenByDescending(team => team.CompletionPercentage)
            .ToList()
            ?? new List<TeamProductivityItemDto>();
    }

    private string GetRiskLabel()
    {
        var riskCount = _data.TotalBlockedWorkItems + _data.TotalOverdueWorkItems;

        return riskCount == 0 ? "No Risk" : riskCount >= 10 ? "High Risk" : "Needs Review";
    }

    private string GetRiskSoftColor()
    {
        var riskCount = _data.TotalBlockedWorkItems + _data.TotalOverdueWorkItems;

        return riskCount == 0 ? ReportPalette.AccentSoft : riskCount >= 10 ? ReportPalette.DangerSoft : ReportPalette.WarningSoft;
    }

    private string GetRiskAccentColor()
    {
        var riskCount = _data.TotalBlockedWorkItems + _data.TotalOverdueWorkItems;

        return riskCount == 0 ? ReportPalette.Accent : riskCount >= 10 ? ReportPalette.Danger : ReportPalette.Warning;
    }

    private static string GetRiskColor(int riskCount)
    {
        return riskCount == 0 ? ReportPalette.Accent : riskCount >= 5 ? ReportPalette.Danger : ReportPalette.Warning;
    }
}