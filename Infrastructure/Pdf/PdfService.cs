using Application.Features.Reports.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Pdf;

public class PdfService : IPdfService
{
    private const string Ink = "#0F172A";
    private const string Slate = "#475569";
    private const string Muted = "#94A3B8";
    private const string Border = "#E2E8F0";
    private const string Soft = "#F8FAFC";
    private const string White = "#FFFFFF";

    private const string Primary = "#2563EB";
    private const string PrimarySoft = "#DBEAFE";
    private const string Accent = "#14B8A6";
    private const string Warning = "#F59E0B";
    private const string Danger = "#EF4444";

    public byte[] GeneratePdf(CycleTimeReport data, byte[] chart1, byte[] chart2, byte[] chart3)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(24);
                page.PageColor(White);

                page.DefaultTextStyle(x =>
                    x.FontSize(10)
                     .FontColor(Ink)
                     .FontFamily(Fonts.Arial));

                page.Header().Element(c => BuildHeader(c, data));

                page.Content().PaddingTop(18).Column(col =>
                {
                    col.Spacing(16);

                    col.Item().Row(row =>
                    {
                        row.Spacing(16);

                        row.RelativeItem(8).Element(c => BuildHeroCard(c, data));
                        row.RelativeItem(4).Element(c => BuildInsightsCard(c, data));
                    });

                    col.Item().Element(c => BuildKpiRow(c, data));

                    col.Item().Element(c => BuildMainChartCard(c, chart1));

                    col.Item().Row(row =>
                    {
                        row.Spacing(16);

                        row.RelativeItem().Element(c => BuildChartCard(c, "Distribution", chart2));
                        row.RelativeItem().Element(c => BuildChartCard(c, "Summary Metrics", chart3));
                    });

                    col.Item().Row(row =>
                    {
                        row.Spacing(16);

                        row.RelativeItem(5).Element(c => BuildTopSlowestCard(c, data));
                        row.RelativeItem(7).Element(c => BuildTableSection(c, data));
                    });
                });

                page.Footer().Element(BuildFooter);
            });
        }).GeneratePdf();
    }

    private void BuildHeader(IContainer container, CycleTimeReport data)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Cycle Time Report")
                    .FontSize(24)
                    .Bold()
                    .FontColor(Ink);

                col.Item().PaddingTop(3).Text("Modern delivery analytics snapshot")
                    .FontSize(10)
                    .FontColor(Muted);
            });

            row.ConstantItem(220).AlignRight().Column(col =>
            {
                col.Item().AlignRight().Text($"Project {data.ProjectKey}")
                    .FontSize(12)
                    .SemiBold()
                    .FontColor(Slate);

                col.Item().AlignRight().Text(data.GeneratedAt.ToString("dd MMM yyyy"))
                    .FontSize(10)
                    .FontColor(Muted);
            });
        });
    }

    private void BuildHeroCard(IContainer container, CycleTimeReport data)
    {
        container
            .Border(1)
            .BorderColor(Border)
            .Background(Soft)
            .Padding(18)
            .Column(col =>
            {
                col.Spacing(10);

                col.Item().Text("Delivery Performance")
                    .FontSize(12)
                    .SemiBold()
                    .FontColor(Primary);

                col.Item().Text($"{data.AverageCycleTimeDays:N1} days")
                    .FontSize(30)
                    .Bold()
                    .FontColor(Ink);

                col.Item().Text(
                    $"Average cycle time across {data.TotalItemsAnalyzed:N0} completed items. " +
                    $"Median is {data.MedianCycleTimeDays:N1} days and the maximum observed cycle time is {data.MaxCycleTimeDays:N1} days.")
                    .FontSize(10.5f)
                    .FontColor(Slate);

                col.Item().Row(row =>
                {
                    row.Spacing(8);

                    row.AutoItem().Element(c => BuildPill(c, "Average", PrimarySoft, Primary));
                    row.AutoItem().Element(c => BuildPill(c, "Distribution", "#ECFEFF", Accent));
                    row.AutoItem().Element(c => BuildPill(c, "Outliers", "#FEF3C7", Warning));
                });
            });
    }

    private void BuildInsightsCard(IContainer container, CycleTimeReport data)
    {
        var ordered = data.Items?
            .OrderByDescending(x => x.CycleTimeDays)
            .Take(3)
            .ToList() ?? new List<WorkItemCycleTime>();

        container
            .Border(1)
            .BorderColor(Border)
            .Background(White)
            .Padding(18)
            .Column(col =>
            {
                col.Spacing(10);

                col.Item().Text("Highlights")
                    .FontSize(12)
                    .SemiBold()
                    .FontColor(Ink);

                col.Item().Element(c => BuildInsightLine(c, "Average", $"{data.AverageCycleTimeDays:N1} days"));
                col.Item().Element(c => BuildInsightLine(c, "Median", $"{data.MedianCycleTimeDays:N1} days"));
                col.Item().Element(c => BuildInsightLine(c, "Range", $"{data.MinCycleTimeDays:N1} – {data.MaxCycleTimeDays:N1} days"));

                if (ordered.Count > 0)
                {
                    col.Item().PaddingTop(4).Text("Slowest items")
                        .FontSize(10)
                        .SemiBold()
                        .FontColor(Slate);

                    foreach (var item in ordered)
                    {
                        col.Item().Text($"{item.Code} • {item.CycleTimeDays:N0} days")
                            .FontSize(9.5f)
                            .FontColor(Muted);
                    }
                }
            });
    }

    private void BuildKpiRow(IContainer container, CycleTimeReport data)
    {
        container.Row(row =>
        {
            row.Spacing(12);

            row.RelativeItem().Component(new ModernStatCard("Average", $"{data.AverageCycleTimeDays:N1}d", Primary, PrimarySoft));
            row.RelativeItem().Component(new ModernStatCard("Median", $"{data.MedianCycleTimeDays:N1}d", Accent, "#ECFEFF"));
            row.RelativeItem().Component(new ModernStatCard("Minimum", $"{data.MinCycleTimeDays:N1}d", "#6366F1", "#EEF2FF"));
            row.RelativeItem().Component(new ModernStatCard("Maximum", $"{data.MaxCycleTimeDays:N1}d", Danger, "#FEE2E2"));
            row.RelativeItem().Component(new ModernStatCard("Items", $"{data.TotalItemsAnalyzed:N0}", Warning, "#FEF3C7"));
        });
    }

    private void BuildMainChartCard(IContainer container, byte[] image)
    {
        container
            .Border(1)
            .BorderColor(Border)
            .Background(White)
            .Padding(16)
            .Column(col =>
            {
                col.Spacing(10);

                col.Item().Text("Cycle Time Ranking")
                    .FontSize(13)
                    .SemiBold()
                    .FontColor(Ink);

                col.Item().Text("Top 12 slowest items ranked by cycle time")
                    .FontSize(9.5f)
                    .FontColor(Muted);

                col.Item().PaddingTop(4).Image(image).FitWidth();
            });
    }

    private void BuildChartCard(IContainer container, string title, byte[] image)
    {
        container
            .Border(1)
            .BorderColor(Border)
            .Background(White)
            .Padding(14)
            .Column(col =>
            {
                col.Spacing(8);

                col.Item().Text(title)
                    .FontSize(12)
                    .SemiBold()
                    .FontColor(Ink);

                col.Item().Image(image).FitWidth();
            });
    }

    private void BuildTopSlowestCard(IContainer container, CycleTimeReport data)
    {
        var ordered = data.Items?
            .OrderByDescending(x => x.CycleTimeDays)
            .Take(5)
            .ToList() ?? new List<WorkItemCycleTime>();

        container
            .Border(1)
            .BorderColor(Border)
            .Background(White)
            .Padding(16)
            .Column(col =>
            {
                col.Spacing(10);

                col.Item().Text("Top Slowest Items")
                    .FontSize(12)
                    .SemiBold()
                    .FontColor(Ink);

                if (ordered.Count == 0)
                {
                    col.Item().Text("No data available")
                        .FontSize(10)
                        .FontColor(Muted);
                    return;
                }

                foreach (var item in ordered)
                {
                    col.Item().Element(c =>
                    {
                        c.BorderBottom(1)
                            .BorderColor(Border)
                            .PaddingBottom(8)
                            .PaddingTop(2)
                            .Row(row =>
                            {
                                row.RelativeItem().Column(inner =>
                                {
                                    inner.Item().Text(item.Code)
                                        .FontSize(10.5f)
                                        .SemiBold()
                                        .FontColor(Ink);

                                    inner.Item().Text(item.EndDate.ToString("dd MMM yyyy"))
                                        .FontSize(9)
                                        .FontColor(Muted);
                                });

                                row.AutoItem().AlignMiddle().Text($"{item.CycleTimeDays:N0}d")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(Danger);
                            });
                    });
                }
            });
    }

    private void BuildTableSection(IContainer container, CycleTimeReport data)
    {
        var orderedItems = data.Items?
            .OrderByDescending(x => x.CycleTimeDays)
            .ToList() ?? new List<WorkItemCycleTime>();

        container
            .Border(1)
            .BorderColor(Border)
            .Background(White)
            .Padding(16)
            .Column(col =>
            {
                col.Spacing(10);

                col.Item().Text("Detailed Work Items")
                    .FontSize(12)
                    .SemiBold()
                    .FontColor(Ink);

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
                        header.Cell().Background(Soft).Padding(10).Text("Item").SemiBold().FontColor(Slate);
                        header.Cell().Background(Soft).Padding(10).Text("Completed").SemiBold().FontColor(Slate);
                        header.Cell().Background(Soft).Padding(10).AlignRight().Text("Cycle Time").SemiBold().FontColor(Slate);
                    });

                    for (int i = 0; i < orderedItems.Count; i++)
                    {
                        var item = orderedItems[i];
                        var bg = i % 2 == 0 ? White : Soft;

                        table.Cell().Background(bg).Padding(10).Text(item.Code).FontColor(Ink);
                        table.Cell().Background(bg).Padding(10).Text(item.EndDate.ToString("dd MMM yyyy")).FontColor(Slate);
                        table.Cell().Background(bg).Padding(10).AlignRight().Text($"{item.CycleTimeDays:N0}d").SemiBold().FontColor(Ink);
                    }
                });
            });
    }

    private void BuildPill(IContainer container, string text, string bg, string fg)
    {
        container
            .Background(bg)
            .PaddingHorizontal(10)
            .PaddingVertical(5)
            .Text(text)
            .FontSize(8.5f)
            .SemiBold()
            .FontColor(fg);
    }

    private void BuildInsightLine(IContainer container, string label, string value)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text(label)
                .FontSize(9.5f)
                .FontColor(Muted);

            row.AutoItem().Text(value)
                .FontSize(9.5f)
                .SemiBold()
                .FontColor(Ink);
        });
    }

    private void BuildFooter(IContainer container)
    {
        container.PaddingTop(8).BorderTop(1).BorderColor(Border).Row(row =>
        {
            row.RelativeItem().Text("Generated by Gemini Analytics Platform")
                .FontSize(9)
                .FontColor(Muted);

            row.AutoItem().Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(9).FontColor(Muted));
                text.Span("Page ");
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        });
    }
}

public class ModernStatCard : IComponent
{
    private readonly string _title;
    private readonly string _value;
    private readonly string _accent;
    private readonly string _soft;

    public ModernStatCard(string title, string value, string accent, string soft)
    {
        _title = title;
        _value = value;
        _accent = accent;
        _soft = soft;
    }

    public void Compose(IContainer container)
    {
        container
            .Border(1)
            .BorderColor("#E2E8F0")
            .Background("#FFFFFF")
            .Padding(14)
            .Column(col =>
            {
                col.Spacing(8);

                col.Item().Row(row =>
                {
                    row.AutoItem().Background(_soft).PaddingHorizontal(8).PaddingVertical(4).Text(_title.ToUpper())
                        .FontSize(8)
                        .SemiBold()
                        .FontColor(_accent);
                });

                col.Item().Text(_value)
                    .FontSize(18)
                    .Bold()
                    .FontColor("#0F172A");
            });
    }
}