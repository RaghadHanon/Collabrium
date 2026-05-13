using Application.Features.Reports.DTOs;
using Domain.Enums;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.PdfComponents;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.OverdueWorkItems;

public class OverdueWorkItemsReportDocument : IDocument
{
    private const int MaxTableRows = 14;

    private readonly OverdueWorkItemsReportDto _data;
    private readonly byte[] _priorityChart;
    private readonly byte[] _statusChart;
    private readonly byte[] _daysOverdueChart;
    private readonly byte[]? _logoBytes;

    public OverdueWorkItemsReportDocument(
        OverdueWorkItemsReportDto data,
        byte[] priorityChart,
        byte[] statusChart,
        byte[] daysOverdueChart,
        byte[]? logoBytes = null)
    {
        _data = data;
        _priorityChart = priorityChart;
        _statusChart = statusChart;
        _daysOverdueChart = daysOverdueChart;
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
            title: "Overdue Work Items Report",
            subtitle: $"Delivery delay analytics • Period: {_data.From:dd MMM yyyy} → {_data.To:dd MMM yyyy}",
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
                title: "Overdue Items by Priority",
                subtitle: $"All {_data.TotalOverdueItems:N0} overdue items grouped by priority",
                image: _priorityChart));

            col.Item().Row(row =>
            {
                row.Spacing(16);

                row.RelativeItem().Component(new SectionCard(
                    title: "Overdue Items by Status",
                    subtitle: "Where overdue work is currently located in the workflow",
                    image: _statusChart));

                row.RelativeItem().Component(new SectionCard(
                    title: "Most Overdue Items",
                    subtitle: "Items ranked by days overdue",
                    image: _daysOverdueChart));
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
        var criticalAndHigh = _data.CriticalOverdueItems + _data.HighOverdueItems;
        var criticalAndHighPercentage = CalculatePercentage(criticalAndHigh, _data.TotalOverdueItems);

        container.ApplySoftCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Delay Overview")
                .Style(ReportTextStyles.CardTitle)
                .FontColor(ReportPalette.Primary);

            col.Item().Text($"{_data.TotalOverdueItems:N0} overdue items")
                .FontSize(30)
                .Bold()
                .FontColor(ReportPalette.Ink);

            col.Item().Text(
                    $"From {_data.From:dd MMM yyyy} to {_data.To:dd MMM yyyy}, the report found {_data.TotalOverdueItems:N0} overdue work items. " +
                    $"{criticalAndHigh:N0} items are critical or high priority, representing {criticalAndHighPercentage:N1}% of all overdue work. " +
                    $"Average delay is {_data.AverageDaysOverdue:N1} days.")
                .Style(ReportTextStyles.Body);

            col.Item().Row(row =>
            {
                row.Spacing(8);

                row.AutoItem().Component(new PillBadge(
                    "Overdue",
                    ReportPalette.PrimarySoft,
                    ReportPalette.Primary));

                row.AutoItem().Component(new PillBadge(
                    "Schedule Risk",
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
        var mostOverdue = GetRankedItems().FirstOrDefault();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Highlights")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Component(new InsightRow("Period", $"{_data.From:dd MMM} - {_data.To:dd MMM yyyy}"));
            col.Item().Component(new InsightRow("Scope", GetScopeLabel()));
            col.Item().Component(new InsightRow("Overdue Items", $"{_data.TotalOverdueItems:N0}"));
            col.Item().Component(new InsightRow("Critical", $"{_data.CriticalOverdueItems:N0}"));
            col.Item().Component(new InsightRow("High", $"{_data.HighOverdueItems:N0}"));
            col.Item().Component(new InsightRow("Avg Delay", $"{_data.AverageDaysOverdue:N1} days"));

            if (mostOverdue is not null)
            {
                col.Item().PaddingTop(4).Text("Most overdue")
                    .Style(ReportTextStyles.Caption)
                    .SemiBold()
                    .FontColor(ReportPalette.Slate);

                col.Item().Text($"{mostOverdue.Code} • {mostOverdue.DaysOverdue:N0} days")
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
                "Overdue",
                $"{_data.TotalOverdueItems:N0}",
                ReportPalette.Primary,
                ReportPalette.PrimarySoft));

            row.RelativeItem().Component(new StatCard(
                "Critical",
                $"{_data.CriticalOverdueItems:N0}",
                ReportPalette.Danger,
                ReportPalette.DangerSoft));

            row.RelativeItem().Component(new StatCard(
                "High",
                $"{_data.HighOverdueItems:N0}",
                ReportPalette.Warning,
                ReportPalette.WarningSoft));

            row.RelativeItem().Component(new StatCard(
                "Effort",
                $"{_data.TotalOverdueEffortPoints:N0}",
                ReportPalette.Indigo,
                ReportPalette.IndigoSoft));

            row.RelativeItem().Component(new StatCard(
                "Avg Delay",
                $"{_data.AverageDaysOverdue:N1}d",
                ReportPalette.Accent,
                ReportPalette.AccentSoft));
        });
    }

    private void BuildRiskBreakdownCard(IContainer container)
    {
        var criticalAndHigh = _data.CriticalOverdueItems + _data.HighOverdueItems;
        var lowerRisk = _data.TotalOverdueItems - criticalAndHigh;

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Delay Risk Breakdown")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Component(new InsightRow(
                "High-Risk Overdue",
                $"{criticalAndHigh:N0} • {CalculatePercentage(criticalAndHigh, _data.TotalOverdueItems):N1}%"));

            col.Item().Component(new InsightRow(
                "Lower-Risk Overdue",
                $"{lowerRisk:N0} • {CalculatePercentage(lowerRisk, _data.TotalOverdueItems):N1}%"));

            col.Item().Component(new InsightRow(
                "Overdue Effort",
                $"{_data.TotalOverdueEffortPoints:N0} points"));

            col.Item().Component(new InsightRow(
                "Average Delay",
                $"{_data.AverageDaysOverdue:N1} days"));

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

            col.Item().Text("Overdue Work Item Details")
                .Style(ReportTextStyles.CardTitle);

            if (items.Count == 0)
            {
                col.Item().Text("No overdue work items available")
                    .Style(ReportTextStyles.Caption);
                return;
            }

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.3f);
                    columns.RelativeColumn(2.5f);
                    columns.RelativeColumn(1.3f);
                    columns.RelativeColumn(1.2f);
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
                    header.Cell().Background(ReportPalette.Soft).Padding(8).Text("Status").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Effort").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(8).AlignRight().Text("Delay").Style(ReportTextStyles.TableHeader);
                });

                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var bg = i % 2 == 0 ? ReportPalette.White : ReportPalette.Soft;

                    table.Cell().Background(bg).Padding(8).Text(item.Code).SemiBold().FontColor(ReportPalette.Ink);
                    table.Cell().Background(bg).Padding(8).Text(Shorten(item.Title, 38)).FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(8).Text(item.AssignedToUserName ?? "-").FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(8).Text(item.Priority.ToString()).SemiBold().FontColor(GetPriorityColor(item.Priority));
                    table.Cell().Background(bg).Padding(8).Text(item.Status.ToString()).FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{item.EffortPoints:N0}").FontColor(ReportPalette.Ink);
                    table.Cell().Background(bg).Padding(8).AlignRight().Text($"{item.DaysOverdue:N0}d").SemiBold().FontColor(ReportPalette.Danger);
                }
            });
        });
    }

    private void BuildFooter(IContainer container)
    {
        ReportLayoutHelpers.BuildStandardFooter(container, "Generated by Collabrium Platform");
    }

    private List<OverdueWorkItemDto> GetRankedItems()
    {
        return _data.Items?
            .OrderByDescending(item => item.Priority)
            .ThenByDescending(item => item.DaysOverdue)
            .ToList()
            ?? new List<OverdueWorkItemDto>();
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
        var highRisk = _data.CriticalOverdueItems + _data.HighOverdueItems;

        return _data.TotalOverdueItems == 0 ? "On Track" : highRisk >= 5 || _data.AverageDaysOverdue >= 7 ? "High Risk" : "Needs Review";
    }

    private string GetRiskSoftColor()
    {
        var highRisk = _data.CriticalOverdueItems + _data.HighOverdueItems;

        return _data.TotalOverdueItems == 0
            ? ReportPalette.AccentSoft
            : highRisk >= 5 || _data.AverageDaysOverdue >= 7 ? ReportPalette.DangerSoft : ReportPalette.WarningSoft;
    }

    private string GetRiskAccentColor()
    {
        var highRisk = _data.CriticalOverdueItems + _data.HighOverdueItems;

        return _data.TotalOverdueItems == 0
            ? ReportPalette.Accent
            : highRisk >= 5 || _data.AverageDaysOverdue >= 7 ? ReportPalette.Danger : ReportPalette.Warning;
    }

    private string GetRiskNarrative()
    {
        if (_data.TotalOverdueItems == 0)
            return "No overdue work items were found for this period.";

        var highRisk = _data.CriticalOverdueItems + _data.HighOverdueItems;
        var highRiskPercentage = CalculatePercentage(highRisk, _data.TotalOverdueItems);

        return highRiskPercentage >= 50
            ? "A large share of overdue items are critical or high priority. These items should be reviewed immediately."
            : _data.AverageDaysOverdue >= 7
            ? "Overdue items are aging. Review timelines, ownership, and dependency risks."
            : "Overdue work exists but appears manageable. Continue monitoring high-priority delayed items.";
    }

    private static double CalculatePercentage(int value, int total)
    {
        return total > 0 ? (double) value / total * 100 : 0;
    }

    private static string GetPriorityColor(WorkItemPriority priority)
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

    private static string Shorten(string value, int maxLength)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "-"
            : value.Length <= maxLength
            ? value
            : value[..maxLength] + "…";
    }
}