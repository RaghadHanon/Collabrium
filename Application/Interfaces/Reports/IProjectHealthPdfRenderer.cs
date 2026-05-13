using Application.Features.Reports.DTOs;

namespace Application.Interfaces.Reports;

public interface IProjectHealthPdfRenderer : IPdfReportRenderer<ProjectHealthReportDto>
{
}
