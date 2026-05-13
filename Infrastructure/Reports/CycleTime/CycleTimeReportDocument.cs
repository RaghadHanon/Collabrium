using Application.Features.Reports.DTOs;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.PdfComponents;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.CycleTime;

public class CycleTimeReportDocument : IDocument
{
    private readonly CycleTimeReport _data;
    private readonly byte[] _mainChart;
    private readonly byte[] _distributionChart;
    private readonly byte[] _summaryChart;
    private readonly byte[]? _logoBytes;
    public CycleTimeReportDocument(
        CycleTimeReport data,
        byte[] mainChart,
        byte[] distributionChart,
        byte[] summaryChart,
        byte[]? logoBytes = null)
    {
        _data = data;
        _mainChart = mainChart;
        _distributionChart = distributionChart;
        _summaryChart = summaryChart;
        _logoBytes = logoBytes;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

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
        ReportLayoutHelpers.BuildStandardHeader(
            container,
            title: "Cycle Time Report",
            subtitle: $"Modern delivery analytics snapshot • Period: {_data.From:dd MMM yyyy} → {_data.To:dd MMM yyyy}",
            rightTop: $"Project {_data.ProjectKey}",
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
                title: "Cycle Time Ranking",
                subtitle: $"All {_data.TotalItemsAnalyzed:N0} items ranked by cycle time",
                image: _mainChart));

            col.Item().Row(row =>
            {
                row.Spacing(16);

                row.RelativeItem().Component(new SectionCard(
                    title: "Distribution",
                    image: _distributionChart));

                row.RelativeItem().Component(new SectionCard(
                    title: "Summary Metrics",
                    image: _summaryChart));
            });

            col.Item().Row(row =>
            {
                row.Spacing(16);

                row.RelativeItem(5).Element(BuildTopSlowestCard);
                row.RelativeItem(7).Element(BuildTableSection);
            });
        });
    }

    private void BuildHeroCard(IContainer container)
    {
        container.ApplySoftCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Delivery Performance")
                .Style(ReportTextStyles.CardTitle)
                .FontColor(ReportPalette.Primary);

            col.Item().Text($"{_data.AverageCycleTimeDays:N1} days")
                .FontSize(30)
                .Bold()
                .FontColor(ReportPalette.Ink);

            col.Item().Text(
                $"From {_data.From:dd MMM yyyy} to {_data.To:dd MMM yyyy}, the report analyzed {_data.TotalItemsAnalyzed:N0} completed items. " +
                $"Median is {_data.MedianCycleTimeDays:N1} days, minimum is {_data.MinCycleTimeDays:N1} days, and maximum is {_data.MaxCycleTimeDays:N1} days.")
                .Style(ReportTextStyles.Body);

            col.Item().Row(row =>
            {
                row.Spacing(8);
                row.AutoItem().Component(new PillBadge("Average", ReportPalette.PrimarySoft, ReportPalette.Primary));
                row.AutoItem().Component(new PillBadge("Distribution", ReportPalette.AccentSoft, ReportPalette.Accent));
                row.AutoItem().Component(new PillBadge("Full Range", ReportPalette.WarningSoft, ReportPalette.Warning));
            });
        });
    }

    private void BuildInsightsCard(IContainer container)
    {
        var ordered = _data.Items?
            .OrderByDescending(x => x.CycleTimeDays)
            .Take(3)
            .ToList() ?? new List<WorkItemCycleTime>();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Highlights").Style(ReportTextStyles.CardTitle);

            col.Item().Component(new InsightRow("Period", $"{_data.From:dd MMM} - {_data.To:dd MMM yyyy}"));
            col.Item().Component(new InsightRow("Average", $"{_data.AverageCycleTimeDays:N1} days"));
            col.Item().Component(new InsightRow("Median", $"{_data.MedianCycleTimeDays:N1} days"));
            col.Item().Component(new InsightRow("Range", $"{_data.MinCycleTimeDays:N1} – {_data.MaxCycleTimeDays:N1} days"));

            if (ordered.Count > 0)
            {
                col.Item().PaddingTop(4).Text("Slowest items")
                    .Style(ReportTextStyles.Caption)
                    .SemiBold()
                    .FontColor(ReportPalette.Slate);

                foreach (var item in ordered)
                {
                    col.Item().Text($"{item.Code} • {item.CycleTimeDays:N0} days")
                        .Style(ReportTextStyles.Label);
                }
            }
        });
    }

    private void BuildKpiRow(IContainer container)
    {
        container.Row(row =>
        {
            row.Spacing(12);

            row.RelativeItem().Component(new StatCard("Average", $"{_data.AverageCycleTimeDays:N1}d", ReportPalette.Primary, ReportPalette.PrimarySoft));
            row.RelativeItem().Component(new StatCard("Median", $"{_data.MedianCycleTimeDays:N1}d", ReportPalette.Accent, ReportPalette.AccentSoft));
            row.RelativeItem().Component(new StatCard("Minimum", $"{_data.MinCycleTimeDays:N1}d", ReportPalette.Indigo, ReportPalette.IndigoSoft));
            row.RelativeItem().Component(new StatCard("Maximum", $"{_data.MaxCycleTimeDays:N1}d", ReportPalette.Danger, ReportPalette.DangerSoft));
            row.RelativeItem().Component(new StatCard("Items", $"{_data.TotalItemsAnalyzed:N0}", ReportPalette.Warning, ReportPalette.WarningSoft));
        });
    }

    private void BuildTopSlowestCard(IContainer container)
    {
        var ordered = _data.Items?
            .OrderByDescending(x => x.CycleTimeDays)
            .Take(5)
            .ToList() ?? new List<WorkItemCycleTime>();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Top Slowest Items")
                .Style(ReportTextStyles.CardTitle);

            if (ordered.Count == 0)
            {
                col.Item().Text("No data available")
                    .Style(ReportTextStyles.Caption);
                return;
            }

            foreach (var item in ordered)
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
                                inner.Item().Text(item.Code)
                                    .FontSize(10.5f)
                                    .SemiBold()
                                    .FontColor(ReportPalette.Ink);

                                inner.Item().Text(item.EndDate.ToString("dd MMM yyyy"))
                                    .Style(ReportTextStyles.Caption);
                            });

                            row.AutoItem().AlignMiddle().Text($"{item.CycleTimeDays:N0}d")
                                .FontSize(11)
                                .Bold()
                                .FontColor(ReportPalette.Danger);
                        });
                });
            }
        });
    }

    private void BuildTableSection(IContainer container)
    {
        var orderedItems = _data.Items?
            .OrderByDescending(x => x.CycleTimeDays)
            .ToList() ?? new List<WorkItemCycleTime>();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Detailed Work Items")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(90);
                });

                table.Header(header =>
                {
                    header.Cell().Background(ReportPalette.Soft).Padding(10).Text("Item").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(10).Text("Completed").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(10).AlignRight().Text("Cycle Time").Style(ReportTextStyles.TableHeader);
                });

                for (int i = 0; i < orderedItems.Count; i++)
                {
                    var item = orderedItems[i];
                    var bg = i % 2 == 0 ? ReportPalette.White : ReportPalette.Soft;

                    table.Cell().Background(bg).Padding(10).Text(item.Code).FontColor(ReportPalette.Ink);
                    table.Cell().Background(bg).Padding(10).Text(item.EndDate.ToString("dd MMM yyyy")).FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(10).AlignRight().Text($"{item.CycleTimeDays:N0}d").SemiBold().FontColor(ReportPalette.Ink);
                }
            });
        });
    }

    private void BuildFooter(IContainer container)
    {
        ReportLayoutHelpers.BuildStandardFooter(container, "Generated by Collabrium Platform");
    }
}