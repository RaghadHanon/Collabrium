using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.Common.PdfComponents;

public class PillBadge : IComponent
{
    private readonly string _text;
    private readonly string _background;
    private readonly string _foreground;

    public PillBadge(string text, string background, string foreground)
    {
        _text = text;
        _background = background;
        _foreground = foreground;
    }

    public void Compose(IContainer container)
    {
        container
            .Background(_background)
            .PaddingHorizontal(10)
            .PaddingVertical(5)
            .Text(_text)
            .Style(ReportTextStyles.Pill)
            .FontColor(_foreground);
    }
}