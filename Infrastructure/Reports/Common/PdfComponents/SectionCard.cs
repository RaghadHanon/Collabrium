using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.Common.PdfComponents;

public class SectionCard : IComponent
{
    private readonly string _title;
    private readonly string? _subtitle;
    private readonly byte[]? _image;
    private readonly string _background;

    public SectionCard(string title, string? subtitle = null, byte[]? image = null, string? background = null)
    {
        _title = title;
        _subtitle = subtitle;
        _image = image;
        _background = background ?? ReportPalette.White;
    }

    public void Compose(IContainer container)
    {
        container
            .Border(1)
            .BorderColor(ReportPalette.Border)
            .Background(_background)
            .Padding(14)
            .Column(col =>
            {
                col.Spacing(8);

                col.Item().Text(_title).Style(ReportTextStyles.SectionTitle);

                if (!string.IsNullOrWhiteSpace(_subtitle))
                {
                    col.Item().Text(_subtitle!).Style(ReportTextStyles.Caption);
                }

                if (_image is not null)
                {
                    col.Item().PaddingTop(4).Image(_image).FitWidth();
                }
            });
    }
}