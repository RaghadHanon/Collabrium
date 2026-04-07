using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.Common.PdfComponents;

public class StatCard : IComponent
{
    private readonly string _title;
    private readonly string _value;
    private readonly string _accentColor;
    private readonly string _softColor;

    public StatCard(string title, string value, string accentColor, string softColor)
    {
        _title = title;
        _value = value;
        _accentColor = accentColor;
        _softColor = softColor;
    }

    public void Compose(IContainer container)
    {
        container
            .Border(1)
            .BorderColor(ReportPalette.Border)
            .Background(ReportPalette.White)
            .Padding(14)
            .Column(col =>
            {
                col.Spacing(8);

                col.Item().Row(row =>
                {
                    row.AutoItem()
                        .Background(_softColor)
                        .PaddingHorizontal(8)
                        .PaddingVertical(4)
                        .Text(_title.ToUpper())
                        .Style(ReportTextStyles.Pill)
                        .FontColor(_accentColor);
                });

                col.Item()
                    .Text(_value)
                    .Style(ReportTextStyles.StatValue);
            });
    }
}
