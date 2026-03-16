using API.Contracts.Reports;
using Application.Features.Reports.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public class ReportsApiMappingProfile : Profile
{
    public ReportsApiMappingProfile()
    {
        CreateMap<GetCycleTimeReportRequest, GetCycleTimeReport.Query>();
        CreateMap<GetCycleTimeReportRequest, GetCycleTimeReportPdf.Query>();
    }
}
