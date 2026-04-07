using Application.Features.Reports.DTOs;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.PdfComponents;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.SprintProgress;

public class SprintProgressReportDocument : IDocument
{
    private readonly SprintProgressDto _data;
    private readonly byte[] _mainChart;
    private readonly byte[] _progressChart;
    private readonly byte[] _summaryChart;
    private readonly byte[]? _logoBytes;

    public SprintProgressReportDocument(
        SprintProgressDto data,
        byte[] mainChart,
        byte[] progressChart,
        byte[] summaryChart,
        byte[]? logoBytes)
    {
        _data = data;
        _mainChart = mainChart;
        _progressChart = progressChart;
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
            title: "Sprint Progress Report",
            subtitle: $"Progress snapshot for sprint {_data.SprintCode} in project {_data.ProjectName}",
            rightTop: $"Project {_data.ProjectKey}",
            rightBottom: $"Sprint {_data.SprintCode}",
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
                title: "Work Items Overview",
                subtitle: "Total and completed work items for the selected sprint",
                image: _mainChart));

            col.Item().Row(row =>
            {
                row.Spacing(16);

                row.RelativeItem().Component(new SectionCard(
                    title: "Progress Percentages",
                    subtitle: "General completion progress versus user story point progress",
                    image: _progressChart));

                row.RelativeItem().Component(new SectionCard(
                    title: "Sprint Summary",
                    subtitle: "Combined snapshot of item counts and calculated progress metrics",
                    image: _summaryChart));
            });

            col.Item().Element(BuildDetailsCard);
        });
    }

    private void BuildHeroCard(IContainer container)
    {
        container.ApplySoftCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Sprint Performance")
                .Style(ReportTextStyles.CardTitle)
                .FontColor(ReportPalette.Primary);

            col.Item().Text($"{_data.GeneralProgressPercentage:N1}%")
                .FontSize(30)
                .Bold()
                .FontColor(ReportPalette.Ink);

            col.Item().Text(
                $"This report summarizes sprint {_data.SprintCode} in project {_data.ProjectName} ({_data.ProjectKey}). " +
                $"Completed work items are {_data.CompletedWorkItems:N0} out of {_data.TotalWorkItems:N0}. " +
                $"General progress reflects the percentage of completed work items relative to all work items in the sprint, " +
                $"while user story progress reflects the percentage of completed user story points relative to total planned user story points.")
                .Style(ReportTextStyles.Body);

            col.Item().Row(row =>
            {
                row.Spacing(8);
                row.AutoItem().Component(new PillBadge("Execution", ReportPalette.PrimarySoft, ReportPalette.Primary));
                row.AutoItem().Component(new PillBadge("Progress", ReportPalette.AccentSoft, ReportPalette.Accent));
                row.AutoItem().Component(new PillBadge("Story Points", ReportPalette.WarningSoft, ReportPalette.Warning));
            });
        });
    }

    private void BuildHighlightsCard(IContainer container)
    {
        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Highlights")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Component(new InsightRow("Project", _data.ProjectName));
            col.Item().Component(new InsightRow("Project Key", _data.ProjectKey));
            col.Item().Component(new InsightRow("Sprint", _data.SprintCode));
            col.Item().Component(new InsightRow("Total Work Items", $"{_data.TotalWorkItems:N0}"));
            col.Item().Component(new InsightRow("Completed Work Items", $"{_data.CompletedWorkItems:N0}"));
            col.Item().Component(new InsightRow("General Progress", $"{_data.GeneralProgressPercentage:N1}%"));
            col.Item().Component(new InsightRow("User Story Progress", $"{_data.UserStoryProgressPercentage:N1}%"));
        });
    }

    private void BuildKpiRow(IContainer container)
    {
        container.Row(row =>
        {
            row.Spacing(12);

            row.RelativeItem().Component(
                new StatCard("Total Items", $"{_data.TotalWorkItems:N0}", ReportPalette.Primary, ReportPalette.PrimarySoft));

            row.RelativeItem().Component(
                new StatCard("Completed", $"{_data.CompletedWorkItems:N0}", ReportPalette.Accent, ReportPalette.AccentSoft));

            row.RelativeItem().Component(
                new StatCard("General %", $"{_data.GeneralProgressPercentage:N1}%", ReportPalette.Indigo, ReportPalette.IndigoSoft));

            row.RelativeItem().Component(
                new StatCard("User Story %", $"{_data.UserStoryProgressPercentage:N1}%", ReportPalette.Danger, ReportPalette.DangerSoft));
        });
    }

    private void BuildDetailsCard(IContainer container)
    {
        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Sprint Progress Details")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(180);
                });

                table.Header(header =>
                {
                    header.Cell().Background(ReportPalette.Soft).Padding(10).Text("Metric").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(10).AlignRight().Text("Value").Style(ReportTextStyles.TableHeader);
                });

                AddRow(table, "Project Name", _data.ProjectName, 0);
                AddRow(table, "Project Key", _data.ProjectKey, 1);
                AddRow(table, "Sprint Code", _data.SprintCode, 3);
                AddRow(table, "Total Work Items", $"{_data.TotalWorkItems:N0}", 4);
                AddRow(table, "Completed Work Items", $"{_data.CompletedWorkItems:N0}", 5);
                AddRow(table, "General Progress Percentage", $"{_data.GeneralProgressPercentage:N1}%", 6);
                AddRow(table, "User Story Progress Percentage", $"{_data.UserStoryProgressPercentage:N1}%", 7);
            });

            col.Item().PaddingTop(10).Text("Metric Notes")
                .Style(ReportTextStyles.CardTitle);

            col.Item().PaddingTop(4).Text(
                "General Progress Percentage is calculated as: Completed Work Items ÷ Total Work Items × 100. " +
                "It measures overall execution progress based on the count of completed items, regardless of their size.")
                .Style(ReportTextStyles.Body);

            col.Item().PaddingTop(4).Text(
                "User Story Progress Percentage is calculated as: Completed User Story Points ÷ Total User Story Points × 100. " +
                "It measures progress based on delivered user story effort, so it is usually more representative when work items vary in size or complexity.")
                .Style(ReportTextStyles.Body);
        });
    }

    private static void AddRow(TableDescriptor table, string label, string value, int index)
    {
        var bg = index % 2 == 0 ? ReportPalette.White : ReportPalette.Soft;

        table.Cell().Background(bg).Padding(10).Text(label).FontColor(ReportPalette.Ink);
        table.Cell().Background(bg).Padding(10).AlignRight().Text(value).SemiBold().FontColor(ReportPalette.Ink);
    }

    private void BuildFooter(IContainer container)
    {
        ReportLayoutHelpers.BuildStandardFooter(container, "Generated by Collabrium Platform");
    }
}