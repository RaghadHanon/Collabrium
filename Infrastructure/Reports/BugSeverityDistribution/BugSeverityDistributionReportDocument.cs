using Application.Features.Reports.DTOs;
using Domain.Enums;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.PdfComponents;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.BugSeverityDistribution;

public class BugSeverityDistributionReportDocument : IDocument
{
    private readonly BugSeverityDistributionReportDto _data;
    private readonly byte[] _mainChart;
    private readonly byte[] _percentageChart;
    private readonly byte[] _riskMixChart;
    private readonly byte[]? _logoBytes;

    public BugSeverityDistributionReportDocument(
        BugSeverityDistributionReportDto data,
        byte[] mainChart,
        byte[] percentageChart,
        byte[] riskMixChart,
        byte[]? logoBytes = null)
    {
        _data = data;
        _mainChart = mainChart;
        _percentageChart = percentageChart;
        _riskMixChart = riskMixChart;
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
        var scope = !string.IsNullOrWhiteSpace(_data.ProjectKey)
            ? $"Project {_data.ProjectKey}"
            : "All Projects";

        ReportLayoutHelpers.BuildStandardHeader(
            container,
            title: "Bug Severity Distribution Report",
            subtitle: $"Quality risk analytics snapshot • Period: {_data.From:dd MMM yyyy} → {_data.To:dd MMM yyyy}",
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
                title: "Bug Severity Distribution",
                subtitle: $"All {_data.TotalBugs:N0} bugs grouped by severity",
                image: _mainChart));

            col.Item().Row(row =>
            {
                row.Spacing(16);

                row.RelativeItem().Component(new SectionCard(
                    title: "Severity Percentage",
                    subtitle: "Each severity level as a percentage of total bugs",
                    image: _percentageChart));

                row.RelativeItem().Component(new SectionCard(
                    title: "Risk Mix",
                    subtitle: "Critical + High bugs compared with Medium + Low bugs",
                    image: _riskMixChart));
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
        var highRiskBugs = _data.CriticalBugsCount + _data.HighBugsCount;
        var highRiskPercentage = CalculatePercentage(highRiskBugs, _data.TotalBugs);

        container.ApplySoftCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Quality Risk Overview")
                .Style(ReportTextStyles.CardTitle)
                .FontColor(ReportPalette.Primary);

            col.Item().Text($"{_data.TotalBugs:N0} bugs")
                .FontSize(30)
                .Bold()
                .FontColor(ReportPalette.Ink);

            col.Item().Text(
                    $"From {_data.From:dd MMM yyyy} to {_data.To:dd MMM yyyy}, the report found {_data.TotalBugs:N0} bugs. " +
                    $"High-risk bugs represent {highRiskPercentage:N1}% of the total, including {_data.CriticalBugsCount:N0} critical and {_data.HighBugsCount:N0} high-priority bugs.")
                .Style(ReportTextStyles.Body);

            col.Item().Row(row =>
            {
                row.Spacing(8);

                row.AutoItem().Component(new PillBadge(
                    "Bug Quality",
                    ReportPalette.PrimarySoft,
                    ReportPalette.Primary));

                row.AutoItem().Component(new PillBadge(
                    "Severity Mix",
                    ReportPalette.AccentSoft,
                    ReportPalette.Accent));

                row.AutoItem().Component(new PillBadge(
                    "Risk Signal",
                    GetRiskSoftColor(highRiskPercentage),
                    GetRiskAccentColor(highRiskPercentage)));
            });
        });
    }

    private void BuildInsightsCard(IContainer container)
    {
        var highestSeverity = GetHighestSeverityWithData();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Highlights")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Component(new InsightRow("Period", $"{_data.From:dd MMM} - {_data.To:dd MMM yyyy}"));
            col.Item().Component(new InsightRow("Scope", _data.ProjectKey ?? "All Projects"));
            col.Item().Component(new InsightRow("Total Bugs", $"{_data.TotalBugs:N0}"));
            col.Item().Component(new InsightRow("Critical", $"{_data.CriticalBugsCount:N0}"));
            col.Item().Component(new InsightRow("High", $"{_data.HighBugsCount:N0}"));

            col.Item().PaddingTop(4).Text("Main risk signal")
                .Style(ReportTextStyles.Caption)
                .SemiBold()
                .FontColor(ReportPalette.Slate);

            col.Item().Text(highestSeverity)
                .Style(ReportTextStyles.Label);
        });
    }

    private void BuildKpiRow(IContainer container)
    {
        container.Row(row =>
        {
            row.Spacing(12);

            row.RelativeItem().Component(new StatCard(
                "Total Bugs",
                $"{_data.TotalBugs:N0}",
                ReportPalette.Primary,
                ReportPalette.PrimarySoft));

            row.RelativeItem().Component(new StatCard(
                "Critical",
                $"{_data.CriticalBugsCount:N0}",
                ReportPalette.Danger,
                ReportPalette.DangerSoft));

            row.RelativeItem().Component(new StatCard(
                "High",
                $"{_data.HighBugsCount:N0}",
                ReportPalette.Warning,
                ReportPalette.WarningSoft));

            row.RelativeItem().Component(new StatCard(
                "Medium",
                $"{_data.MediumBugsCount:N0}",
                ReportPalette.Indigo,
                ReportPalette.IndigoSoft));

            row.RelativeItem().Component(new StatCard(
                "Low",
                $"{_data.LowBugsCount:N0}",
                ReportPalette.Accent,
                ReportPalette.AccentSoft));
        });
    }

    private void BuildRiskBreakdownCard(IContainer container)
    {
        var highRiskBugs = _data.CriticalBugsCount + _data.HighBugsCount;
        var normalRiskBugs = _data.MediumBugsCount + _data.LowBugsCount;

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Risk Breakdown")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Component(new InsightRow(
                "High Risk",
                $"{highRiskBugs:N0} bugs • {CalculatePercentage(highRiskBugs, _data.TotalBugs):N1}%"));

            col.Item().Component(new InsightRow(
                "Normal Risk",
                $"{normalRiskBugs:N0} bugs • {CalculatePercentage(normalRiskBugs, _data.TotalBugs):N1}%"));

            col.Item().Component(new InsightRow(
                "Critical Share",
                $"{_data.CriticalPercentage:N1}%"));

            col.Item().Component(new InsightRow(
                "High Share",
                $"{_data.HighPercentage:N1}%"));

            col.Item().PaddingTop(6).Text(GetRiskNarrative())
                .Style(ReportTextStyles.Body);
        });
    }

    private void BuildTableSection(IContainer container)
    {
        var items = GetOrderedItems();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Severity Details")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                });

                table.Header(header =>
                {
                    header.Cell().Background(ReportPalette.Soft).Padding(10).Text("Severity").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(10).AlignRight().Text("Count").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(10).AlignRight().Text("Percentage").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(10).Text("Meaning").Style(ReportTextStyles.TableHeader);
                });

                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var bg = i % 2 == 0 ? ReportPalette.White : ReportPalette.Soft;

                    table.Cell().Background(bg).Padding(10).Text(item.Severity.ToString()).SemiBold().FontColor(GetSeverityTextColor(item.Severity));
                    table.Cell().Background(bg).Padding(10).AlignRight().Text($"{item.Count:N0}").FontColor(ReportPalette.Ink);
                    table.Cell().Background(bg).Padding(10).AlignRight().Text($"{item.Percentage:N1}%").FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(10).Text(GetSeverityMeaning(item.Severity)).FontColor(ReportPalette.Slate);
                }
            });
        });
    }

    private void BuildFooter(IContainer container)
    {
        ReportLayoutHelpers.BuildStandardFooter(container, "Generated by Collabrium Platform");
    }

    private List<BugSeverityDistributionItemDto> GetOrderedItems()
    {
        return _data.Items?
            .OrderBy(item => GetSeverityOrder(item.Severity))
            .ToList()
            ?? new List<BugSeverityDistributionItemDto>();
    }

    private string GetHighestSeverityWithData()
    {
        if (_data.CriticalBugsCount > 0)
            return "Critical bugs are present and should be reviewed first.";

        if (_data.HighBugsCount > 0)
            return "High-priority bugs are present and may affect delivery quality.";

        return _data.MediumBugsCount > 0
            ? "Most visible risk is medium severity."
            : _data.LowBugsCount > 0 ? "Only low severity bugs were found in this period." : "No bugs were found in this period.";
    }

    private string GetRiskNarrative()
    {
        if (_data.TotalBugs == 0)
            return "No bugs were reported during this period.";

        var highRiskBugs = _data.CriticalBugsCount + _data.HighBugsCount;
        var highRiskPercentage = CalculatePercentage(highRiskBugs, _data.TotalBugs);

        return highRiskPercentage >= 50
            ? "A large share of bugs are high risk. Prioritize critical and high-priority fixes before release decisions."
            : highRiskPercentage >= 25
            ? "High-risk bugs are noticeable. Review ownership and fix plans for critical and high-priority bugs."
            : "Most bugs are not high risk. Continue monitoring critical and high-priority issues.";
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

    private static string GetSeverityMeaning(WorkItemPriority priority)
    {
        return priority switch
        {
            WorkItemPriority.Low => "Minor quality issue",
            WorkItemPriority.Medium => "Needs planned attention",
            WorkItemPriority.High => "May affect delivery",
            WorkItemPriority.Critical => "Requires urgent review",
            _ => "Unknown severity"
        };
    }

    private static string GetSeverityTextColor(WorkItemPriority priority)
    {
        return priority switch
        {
            WorkItemPriority.Low => ReportPalette.Accent,
            WorkItemPriority.Medium => ReportPalette.Primary,
            WorkItemPriority.High => ReportPalette.Warning,
            WorkItemPriority.Critical => ReportPalette.Danger,
            _ => ReportPalette.Slate
        };
    }

    private static string GetRiskSoftColor(double highRiskPercentage)
    {
        return highRiskPercentage >= 50
            ? ReportPalette.DangerSoft
            : highRiskPercentage >= 25 ? ReportPalette.WarningSoft : ReportPalette.AccentSoft;
    }

    private static string GetRiskAccentColor(double highRiskPercentage)
    {
        return highRiskPercentage >= 50 ? ReportPalette.Danger : highRiskPercentage >= 25 ? ReportPalette.Warning : ReportPalette.Accent;
    }

    private static double CalculatePercentage(int value, int total)
    {
        return total > 0 ? (double) value / total * 100 : 0;
    }
}