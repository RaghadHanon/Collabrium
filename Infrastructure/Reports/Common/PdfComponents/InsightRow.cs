using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.Common.PdfComponents;

public class InsightRow : IComponent
{
    private readonly string _label;
    private readonly string _value;

    public InsightRow(string label, string value)
    {
        _label = label;
        _value = value;
    }

    public void Compose(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem()
                .Text(_label)
                .Style(ReportTextStyles.Label);

            row.AutoItem()
                .Text(_value)
                .Style(ReportTextStyles.Value);
        });
    }
}