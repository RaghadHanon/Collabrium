namespace Application.Interfaces.Reports;
public interface IPdfReportRenderer<in TReport>
{
    byte[] Render(TReport report);
}