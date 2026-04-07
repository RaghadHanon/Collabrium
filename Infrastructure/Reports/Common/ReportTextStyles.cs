using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.Common;

public static class ReportTextStyles
{
    public static TextStyle Base => TextStyle.Default
        .FontFamily(Fonts.Arial)
        .FontSize(10)
        .FontColor(ReportPalette.Ink);

    public static TextStyle Title => Base
        .FontSize(24)
        .Bold()
        .FontColor(ReportPalette.Ink);

    public static TextStyle Subtitle => Base
        .FontSize(10)
        .FontColor(ReportPalette.Muted);

    public static TextStyle SectionTitle => Base
        .FontSize(13)
        .SemiBold()
        .FontColor(ReportPalette.Ink);

    public static TextStyle CardTitle => Base
        .FontSize(12)
        .SemiBold()
        .FontColor(ReportPalette.Ink);

    public static TextStyle Body => Base
        .FontSize(10.5f)
        .FontColor(ReportPalette.Slate);

    public static TextStyle Caption => Base
        .FontSize(9)
        .FontColor(ReportPalette.Muted);

    public static TextStyle Label => Base
        .FontSize(9.5f)
        .FontColor(ReportPalette.Muted);

    public static TextStyle Value => Base
        .FontSize(9.5f)
        .SemiBold()
        .FontColor(ReportPalette.Ink);

    public static TextStyle StatValue => Base
        .FontSize(18)
        .Bold()
        .FontColor(ReportPalette.Ink);

    public static TextStyle Pill => Base
        .FontSize(8.5f)
        .SemiBold();

    public static TextStyle TableHeader => Base
        .FontSize(9.5f)
        .SemiBold()
        .FontColor(ReportPalette.Slate);
}