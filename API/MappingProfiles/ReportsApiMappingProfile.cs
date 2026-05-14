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

        CreateMap<GetBugSeverityDistributionReportRequest, GetBugSeverityDistributionReport.Query>();
        CreateMap<GetBugSeverityDistributionReportRequest, GetBugSeverityDistributionReportPdf.Query>();

        CreateMap<GetDeveloperProductivityReportRequest, GetDeveloperProductivityReport.Query>();
        CreateMap<GetDeveloperProductivityReportRequest, GetDeveloperProductivityReportPdf.Query>();

        CreateMap<GetTeamsProductivityReportRequest, GetTeamsProductivityReport.Query>();
        CreateMap<GetTeamsProductivityReportRequest, GetTeamsProductivityReportPdf.Query>();

        CreateMap<GetBlockedWorkItemsReportRequest, GetBlockedWorkItemsReport.Query>();
        CreateMap<GetBlockedWorkItemsReportRequest, GetBlockedWorkItemsReportPdf.Query>();

        CreateMap<GetOverdueWorkItemsReportRequest, GetOverdueWorkItemsReport.Query>();
        CreateMap<GetOverdueWorkItemsReportRequest, GetOverdueWorkItemsReportPdf.Query>();

        CreateMap<GetProjectHealthReportRequest, GetProjectHealthReport.Query>();
        CreateMap<GetProjectHealthReportRequest, GetProjectHealthReportPdf.Query>();
    }
}