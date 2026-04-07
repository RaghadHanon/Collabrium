using Application.Features.Reports.DTOs;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.PdfComponents;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.Velocity;

public class VelocityReportDocument : IDocument
{
    private readonly VelocityReportDto _data;
    private readonly byte[] _mainChart;
    private readonly byte[] _percentageChart;
    private readonly byte[] _summaryChart;
    private readonly byte[]? _logoBytes;

    public VelocityReportDocument(
        VelocityReportDto data,
        byte[] mainChart,
        byte[] percentageChart,
        byte[] summaryChart,
        byte[]? logoBytes = null)
    {
        _data = data;
        _mainChart = mainChart;
        _percentageChart = percentageChart;
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
            title: "Velocity Report",
            subtitle: $"Sprint delivery and velocity performance snapshot for {_data.ProjectName}",
            rightTop: $"Project {_data.ProjectKey}",
            rightBottom: $"{_data.SprintVelocities.Count:N0} sprints",
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
                row.RelativeItem(4).Element(BuildHighlightsCard);
            });

            col.Item().Element(BuildKpiRow);

            col.Item().Component(new SectionCard(
                title: "Completed Story Points by Sprint",
                subtitle: $"Across {_data.SprintVelocities.Count:N0} sprints",
                image: _mainChart));

            col.Item().Row(row =>
            {
                row.Spacing(16);

                row.RelativeItem().Component(new SectionCard(
                    title: "Velocity Percentage by Sprint",
                    image: _percentageChart));

                row.RelativeItem().Component(new SectionCard(
                    title: "Total vs Completed Story Points",
                    image: _summaryChart));
            });

            col.Item().Row(row =>
            {
                row.Spacing(16);

                row.RelativeItem(5).Element(BuildBestWorstCard);
                row.RelativeItem(7).Element(BuildTableSection);
            });
        });
    }

    private void BuildHeroCard(IContainer container)
    {
        container.ApplySoftCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Velocity Performance")
                .Style(ReportTextStyles.CardTitle)
                .FontColor(ReportPalette.Primary);

            col.Item().Text($"{_data.AvgVelocityPercentage:N1}%")
                .FontSize(30)
                .Bold()
                .FontColor(ReportPalette.Ink);

            col.Item().Text(
                $"This report summarizes sprint velocity for {_data.ProjectName}. " +
                $"Total planned story points are {_data.TotalStoryPoints:N0}, completed story points are {_data.CompletedStoryPoints:N0}, " +
                $"and the average velocity percentage across all sprints is {_data.AvgVelocityPercentage:N1}%.")
                .Style(ReportTextStyles.Body);

            col.Item().Row(row =>
            {
                row.Spacing(8);
                row.AutoItem().Component(new PillBadge("Velocity", ReportPalette.PrimarySoft, ReportPalette.Primary));
                row.AutoItem().Component(new PillBadge("Story Points", ReportPalette.AccentSoft, ReportPalette.Accent));
                row.AutoItem().Component(new PillBadge("Sprint Health", ReportPalette.WarningSoft, ReportPalette.Warning));
            });
        });
    }

    private void BuildHighlightsCard(IContainer container)
    {
        var highest = _data.SprintVelocities
            .OrderByDescending(x => x.VelocityPercentage)
            .FirstOrDefault();

        var lowest = _data.SprintVelocities
            .OrderBy(x => x.VelocityPercentage)
            .FirstOrDefault();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Highlights")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Component(new InsightRow("Project", _data.ProjectName));
            col.Item().Component(new InsightRow("Sprints", $"{_data.SprintVelocities.Count:N0}"));
            col.Item().Component(new InsightRow("Average Velocity", $"{_data.AvgVelocityPercentage:N1}%"));
            col.Item().Component(new InsightRow("Total Story Points", $"{_data.TotalStoryPoints:N0}"));
            col.Item().Component(new InsightRow("Completed Story Points", $"{_data.CompletedStoryPoints:N0}"));

            if (highest is not null)
                col.Item().Component(new InsightRow("Best Sprint", $"{highest.SprintCode} • {highest.VelocityPercentage:N1}%"));

            if (lowest is not null)
                col.Item().Component(new InsightRow("Lowest Sprint", $"{lowest.SprintCode} • {lowest.VelocityPercentage:N1}%"));
        });
    }

    private void BuildKpiRow(IContainer container)
    {
        var sprintCount = _data.SprintVelocities.Count;

        container.Row(row =>
        {
            row.Spacing(12);

            row.RelativeItem().Component(new StatCard("Total SP", $"{_data.TotalStoryPoints:N0}", ReportPalette.Primary, ReportPalette.PrimarySoft));
            row.RelativeItem().Component(new StatCard("Completed SP", $"{_data.CompletedStoryPoints:N0}", ReportPalette.Accent, ReportPalette.AccentSoft));
            row.RelativeItem().Component(new StatCard("Avg Velocity", $"{_data.AvgVelocityPercentage:N1}%", ReportPalette.Indigo, ReportPalette.IndigoSoft));
            row.RelativeItem().Component(new StatCard("Sprints", $"{sprintCount:N0}", ReportPalette.Warning, ReportPalette.WarningSoft));
        });
    }

    private void BuildBestWorstCard(IContainer container)
    {
        var ordered = _data.SprintVelocities
            .OrderByDescending(x => x.VelocityPercentage)
            .ToList();

        var top = ordered.Take(3).ToList();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Top Velocity Sprints")
                .Style(ReportTextStyles.CardTitle);

            if (top.Count == 0)
            {
                col.Item().Text("No data available")
                    .Style(ReportTextStyles.Caption);
                return;
            }

            foreach (var sprint in top)
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
                                inner.Item().Text(sprint.SprintCode)
                                    .FontSize(10.5f)
                                    .SemiBold()
                                    .FontColor(ReportPalette.Ink);

                                inner.Item().Text(
                                        $"Completed {sprint.CompletedStoryPoints:N0} / {sprint.TotalStoryPoints:N0} SP")
                                    .Style(ReportTextStyles.Caption);
                            });

                            row.AutoItem().AlignMiddle().Text($"{sprint.VelocityPercentage:N1}%")
                                .FontSize(11)
                                .Bold()
                                .FontColor(ReportPalette.Accent);
                        });
                });
            }
        });
    }

    private void BuildTableSection(IContainer container)
    {
        var ordered = _data.SprintVelocities.ToList();

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Sprint Velocity Details")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(110);
                    columns.ConstantColumn(130);
                    columns.ConstantColumn(110);
                });

                table.Header(header =>
                {
                    header.Cell().Background(ReportPalette.Soft).Padding(10).Text("Sprint").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(10).AlignRight().Text("Total SP").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(10).AlignRight().Text("Completed SP").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(10).AlignRight().Text("Velocity %").Style(ReportTextStyles.TableHeader);
                });

                for (int i = 0; i < ordered.Count; i++)
                {
                    var sprint = ordered[i];
                    var bg = i % 2 == 0 ? ReportPalette.White : ReportPalette.Soft;

                    table.Cell().Background(bg).Padding(10).Text(sprint.SprintCode).FontColor(ReportPalette.Ink);
                    table.Cell().Background(bg).Padding(10).AlignRight().Text($"{sprint.TotalStoryPoints:N0}").FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(10).AlignRight().Text($"{sprint.CompletedStoryPoints:N0}").FontColor(ReportPalette.Slate);
                    table.Cell().Background(bg).Padding(10).AlignRight().Text($"{sprint.VelocityPercentage:N1}%").SemiBold().FontColor(ReportPalette.Ink);
                }
            });
        });
    }

    private void BuildFooter(IContainer container)
    {
        ReportLayoutHelpers.BuildStandardFooter(container, "Generated by Collabrium Platform");
    }
}