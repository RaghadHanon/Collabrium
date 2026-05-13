using Application.Features.Reports.DTOs;

namespace Application.Interfaces.Reports;

public interface ITeamsProductivityPdfRenderer : IPdfReportRenderer<TeamsProductivityReportDto>
{
}