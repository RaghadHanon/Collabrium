using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.Common;

public static class ReportLayoutHelpers
{
    public static IContainer ApplyPageCard(
        this IContainer container,
        string? background = null)
    {
        return container
            .Border(1)
            .BorderColor(ReportPalette.Border)
            .Background(background ?? ReportPalette.White)
            .Padding(16);
    }

    public static IContainer ApplySoftCard(
        this IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(ReportPalette.Border)
            .Background(ReportPalette.Soft)
            .Padding(16);
    }

    public static void BuildStandardFooter(
        IContainer container,
        string appName)
    {
        container
            .PaddingTop(8)
            .BorderTop(1)
            .BorderColor(ReportPalette.Border)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text(appName)
                    .Style(ReportTextStyles.Caption);

                row.AutoItem()
                    .Text(text =>
                    {
                        text.DefaultTextStyle(ReportTextStyles.Caption);
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
            });
    }

    public static void BuildStandardHeader(
        IContainer container,
        string title,
        string subtitle,
        string rightTop,
        string rightBottom,
        byte[]? logoBytes = null)
    {
        container.Row(row =>
        {
            row.RelativeItem().Row(innerRow =>
            {
                innerRow.Spacing(12);

                if (logoBytes is not null && logoBytes.Length > 0)
                {
                    innerRow.ConstantItem(95).AlignTop().PaddingTop(4).Image(logoBytes).FitWidth();
                }

                innerRow.RelativeItem().Column(col =>
                {
                    col.Spacing(2);

                    col.Item().Text(title)
                        .Style(ReportTextStyles.Title);

                    col.Item().Text(subtitle)
                        .Style(ReportTextStyles.Subtitle);
                });
            });

            row.ConstantItem(190).Column(col =>
            {
                col.Item().AlignRight().Text(rightTop).Style(ReportTextStyles.Value);
                col.Item().AlignRight().Text(rightBottom).Style(ReportTextStyles.Caption);
            });
        });
    }
}