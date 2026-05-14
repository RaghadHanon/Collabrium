using Application.Features.Reports.DTOs;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.PdfComponents;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.BlockedWorkItems;

public class BlockedWorkItemsReportDocument : IDocument
{
    private const int MaxTableRows = 14;

    private readonly BlockedWorkItemsReportDto _data;
    private readonly byte[] _priorityChart;
    private readonly byte[] _statusChart;
    private readonly byte[] _ageChart;
    private readonly byte[]? _logoBytes;

    public BlockedWorkItemsReportDocument(
        BlockedWorkItemsReportDto data,
        byte[] priorityChart,
        byte[] statusChart,
        byte[] ageChart,
        byte[]? logoBytes = null)
    {
        _data = data;
        _priorityChart = priorityChart;
        _statusChart = statusChart;
        _ageChart = ageChart;
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

        if (_data.TeamId is not null)
            scope += $" • Team {_data.TeamId}";

        ReportLayoutHelpers.BuildStandardHeader(
            container,
            title: "Blocked Work Items Report",
            subtitle: $"Delivery blockers analytics • Period: {_data.From:dd MMM yyyy} → {_data.To:dd MMM yyyy}",
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
                title: "Blocked Items by Priority",
                subtitle: $"All {_data.TotalBlockedItems:N0} blocked items grouped by priority",
                image: _priorityChart));

            col.Item().Row(row =>
            {
                row.Spacing(16);

                row.RelativeItem().Component(new SectionCard(
                    title: "Blocked Items by Status",
                    subtitle: "Where blockers are currently located in the workflow",
                    image: _statusChart));

                row.RelativeItem().Component(new SectionCard(
                    title: "Oldest Blocked Items",
                    subtitle: "Items ranked by blocked age",
                    image: _ageChart));
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
        var criticalAndHigh = _data.CriticalBlockedItems + _data.HighBlockedItems;
        var criticalAndHighPercentage = CalculatePercentage(criticalAndHigh, _data.TotalBlockedItems);

        container.ApplySoftCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Blocker Overview")
                .Style(ReportTextStyles.CardTitle)
                .FontColor(ReportPalette.Primary);

            col.Item().Text($"{_data.TotalBlockedItems:N0} blocked items")
                .FontSize(30)
                .Bold()
                .FontColor(ReportPalette.Ink);

            col.Item().Text(
                    $"From {_data.From:dd MMM yyyy} to {_data.To:dd MMM yyyy}, the report found {_data.TotalBlockedItems:N0} blocked work items. " +
                    $"{criticalAndHigh:N0} items are high-risk blockers, representing {criticalAndHighPercentage:N1}% of all blockers. " +
                    $"Average blocked age is {_data.AverageBlockedAgeDays:N1} days.")
                .Style(ReportTextStyles.Body);

            col.Item().Row(row =>
            {
                row.Spacing(8);

                row.AutoItem().Component(new PillBadge(
                    "Blockers",
                    ReportPalette.PrimarySoft,
                    ReportPalette.Primary));

                row.AutoItem().Component(new PillBadge(
                    "Workflow Risk",
                    ReportPalette.WarningSoft,
                    ReportPalette.Warning));

                row.AutoItem().Component(new PillBadge(
                    GetRiskLabel(),
                    GetRiskSoftColor(),
                    GetRiskAccentColor()));
            });
        });
    }

    private void BuildInsightsCard(IContainer container)
    {
        var oldest = GetRankedItems().FirstOrDefault();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Highlights")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Component(new InsightRow("Period", $"{_data.From:dd MMM} - {_data.To:dd MMM yyyy}"));
            col.Item().Component(new InsightRow("Scope", GetScopeLabel()));
            col.Item().Component(new InsightRow("Blocked Items", $"{_data.TotalBlockedItems:N0}"));
            col.Item().Component(new InsightRow("Critical", $"{_data.CriticalBlockedItems:N0}"));
            col.Item().Component(new InsightRow("High", $"{_data.HighBlockedItems:N0}"));
            col.Item().Component(new InsightRow("Avg Age", $"{_data.AverageBlockedAgeDays:N1} days"));

            if (oldest is not null)
            {
                col.Item().PaddingTop(4).Text("Oldest blocker")
                    .Style(ReportTextStyles.Caption)
                    .SemiBold()
                    .FontColor(ReportPalette.Slate);

                col.Item().Text($"{oldest.Code} • {oldest.BlockedAgeDays:N0} days")
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
                "Blocked",
                $"{_data.TotalBlockedItems:N0}",
                ReportPalette.Primary,
                ReportPalette.PrimarySoft));

            row.RelativeItem().Component(new StatCard(
                "Critical",
                $"{_data.CriticalBlockedItems:N0}",
                ReportPalette.Danger,
                ReportPalette.DangerSoft));

            row.RelativeItem().Component(new StatCard(
                "High",
                $"{_data.HighBlockedItems:N0}",
                ReportPalette.Warning,
                ReportPalette.WarningSoft));

            row.RelativeItem().Component(new StatCard(
                "Effort",
                $"{_data.TotalBlockedEffortPoints:N0}",
                ReportPalette.Indigo,
                ReportPalette.IndigoSoft));

            row.RelativeItem().Component(new StatCard(
                "Avg Age",
                $"{_data.AverageBlockedAgeDays:N1}d",
                ReportPalette.Accent,
                ReportPalette.AccentSoft));
        });
    }

    private void BuildRiskBreakdownCard(IContainer container)
    {
        var criticalAndHigh = _data.CriticalBlockedItems + _data.HighBlockedItems;
        var lowerRisk = _data.TotalBlockedItems - criticalAndHigh;

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Blocker Risk Breakdown")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Component(new InsightRow(
                "High-Risk Blockers",
                $"{criticalAndHigh:N0} • {CalculatePercentage(criticalAndHigh, _data.TotalBlockedItems):N1}%"));

            col.Item().Component(new InsightRow(
                "Lower-Risk Blockers",
                $"{lowerRisk:N0} • {CalculatePercentage(lowerRisk, _data.TotalBlockedItems):N1}%"));

            col.Item().Component(new InsightRow(
                "Blocked Effort",
                $"{_data.TotalBlockedEffortPoints:N0} points"));

            col.Item().Component(new InsightRow(
                "Average Age",
                $"{_data.AverageBlockedAgeDays:N1} days"));

            col.Item().PaddingTop(6).Text(GetRiskNarrative())
                .Style(ReportTextStyles.Body);
        });
    }

    private void BuildTableSection(IContainer container)
    {
        var items = GetRankedItems()
            .Take(MaxTableRows)
            .ToList();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Blocked Work Item Details")
                .Style(ReportTextStyles.CardTitle);

            if (items.Count == 0)
            {
                col.Item().Text("No blocked work items available")
                    .Style(ReportTextStyles.Caption);
                return;
            }

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.4f);
                    columns.RelativeColumn(2.6f);
                    columns.RelativeColumn(1.4f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                });

                table.Header(header =>
                {
                    header.Cell().Background(ReportPalette.Soft).Padding(8).Text("Code").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).Text("Title").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).Text("Assignee").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).Text("Priority").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Effort").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Age").Style(ReportTextStyles.TableHeader);
                });

                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var bg = i % 2 == 0 ? ReportPalette.White : ReportPalette.Soft;

                    table.Cell().Background(bg).Padding(8).Text(item.Code).SemiBold().FontColor(ReportPalette.Ink);
                    table.Cell().Background(bg).Padding(8).Text(Shorten(item.Title, 42)).FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(8).Text(item.AssignedToUserName ?? "-").FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(8).Text(item.Priority.ToString()).SemiBold().FontColor(GetPriorityColor(item.Priority));
                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{item.EffortPoints:N0}").FontColor(ReportPalette.Ink);
                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{item.BlockedAgeDays:N0}d").SemiBold().FontColor(ReportPalette.Danger);
                }
            });
        });
    }

    private void BuildFooter(IContainer container)
    {
        ReportLayoutHelpers.BuildStandardFooter(container, "Generated by Collabrium Platform");
    }

    private List<BlockedWorkItemDto> GetRankedItems()
    {
        return _data.Items?
            .OrderByDescending(item => item.Priority)
            .ThenByDescending(item => item.BlockedAgeDays)
            .ToList()
            ?? new List<BlockedWorkItemDto>();
    }

    private string GetScopeLabel()
    {
        if (_data.ProjectId is null && _data.TeamId is null && _data.AssignedToUserId is null)
            return "All Projects";

        var parts = new List<string>();

        if (_data.ProjectId is not null)
            parts.Add($"Project {_data.ProjectId}");

        if (_data.TeamId is not null)
            parts.Add($"Team {_data.TeamId}");

        if (_data.AssignedToUserId is not null)
            parts.Add($"User {_data.AssignedToUserId}");

        return string.Join(" • ", parts);
    }

    private string GetRiskLabel()
    {
        var highRisk = _data.CriticalBlockedItems + _data.HighBlockedItems;

        return _data.TotalBlockedItems == 0
            ? "No Blockers"
            : highRisk >= 5 || _data.AverageBlockedAgeDays >= 7 ? "High Risk" : "Needs Review";
    }

    private string GetRiskSoftColor()
    {
        var highRisk = _data.CriticalBlockedItems + _data.HighBlockedItems;

        return _data.TotalBlockedItems == 0
            ? ReportPalette.AccentSoft
            : highRisk >= 5 || _data.AverageBlockedAgeDays >= 7 ? ReportPalette.DangerSoft : ReportPalette.WarningSoft;
    }

    private string GetRiskAccentColor()
    {
        var highRisk = _data.CriticalBlockedItems + _data.HighBlockedItems;

        return _data.TotalBlockedItems == 0
            ? ReportPalette.Accent
            : highRisk >= 5 || _data.AverageBlockedAgeDays >= 7 ? ReportPalette.Danger : ReportPalette.Warning;
    }

    private string GetRiskNarrative()
    {
        if (_data.TotalBlockedItems == 0)
            return "No blocked work items were found for this period.";

        var highRisk = _data.CriticalBlockedItems + _data.HighBlockedItems;
        var highRiskPercentage = CalculatePercentage(highRisk, _data.TotalBlockedItems);

        return highRiskPercentage >= 50
            ? "A large share of blockers are critical or high priority. These items should be reviewed immediately."
            : _data.AverageBlockedAgeDays >= 7
            ? "Blocked items are aging. Review ownership, dependencies, and escalation paths."
            : "Blocked items are present but risk appears manageable. Continue monitoring high-priority blockers.";
    }

    private static double CalculatePercentage(int value, int total)
    {
        return total > 0 ? (double) value / total * 100 : 0;
    }

    private static string GetPriorityColor(Domain.Enums.WorkItemPriority priority)
    {
        return priority switch
        {
            Domain.Enums.WorkItemPriority.Low => ReportPalette.Accent,
            Domain.Enums.WorkItemPriority.Medium => ReportPalette.Primary,
            Domain.Enums.WorkItemPriority.High => ReportPalette.Warning,
            Domain.Enums.WorkItemPriority.Critical => ReportPalette.Danger,
            _ => ReportPalette.Slate
        };
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