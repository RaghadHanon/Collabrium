using ScottPlot;

namespace Infrastructure.Reports.Common.Charts;

public static class BaseChartFactory
{
    public static Plot CreateModernPlot()
    {
        var plt = new Plot();

        var ink = Color.FromHex(ReportPalette.Ink);
        var slate = Color.FromHex(ReportPalette.Slate);
        var grid = Color.FromHex("#EEF2F7");

        plt.FigureBackground.Color = Colors.White;
        plt.DataBackground.Color = Colors.White;

        plt.Grid.MajorLineColor = grid;
        plt.Grid.MinorLineColor = grid.WithAlpha(0.25);

        plt.Axes.Left.TickLabelStyle.FontSize = 11;
        plt.Axes.Bottom.TickLabelStyle.FontSize = 11;

        plt.Axes.Left.TickLabelStyle.ForeColor = slate;
        plt.Axes.Bottom.TickLabelStyle.ForeColor = slate;

        plt.Axes.Left.Label.ForeColor = slate;
        plt.Axes.Bottom.Label.ForeColor = slate;
        plt.Axes.Title.Label.ForeColor = ink;

        return plt;
    }

    public static void ApplyTitle(Plot plt, string title, int fontSize = 18)
    {
        plt.Axes.Title.Label.Text = title;
        plt.Axes.Title.Label.FontSize = fontSize;
        plt.Axes.Title.Label.Bold = true;
    }

    public static void ApplyAxisLabels(Plot plt, string xLabel, string yLabel)
    {
        plt.Axes.Bottom.Label.Text = xLabel;
        plt.Axes.Left.Label.Text = yLabel;
    }

    public static void ApplyDefaultMargins(Plot plt,
        double left = 0.08,
        double right = 0.05,
        double top = 0.12,
        double bottom = 0.10)
    {
        plt.Axes.Margins(left: left, right: right, top: top, bottom: bottom);
    }

    public static Color Primary()
    {
        return Color.FromHex(ReportPalette.Primary);
    }

    public static Color PrimarySoft()
    {
        return Color.FromHex(ReportPalette.PrimarySoft);
    }

    public static Color Accent()
    {
        return Color.FromHex(ReportPalette.Accent);
    }

    public static Color Danger()
    {
        return Color.FromHex(ReportPalette.Danger);
    }

    public static Color Slate()
    {
        return Color.FromHex(ReportPalette.Slate);
    }
}