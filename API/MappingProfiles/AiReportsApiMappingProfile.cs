using API.Contracts.Reports;
using Application.Features.AI.Queries;
using AutoMapper;
using Domain.Enums;

namespace API.MappingProfiles;

public sealed class AiReportsApiMappingProfile : Profile
{
    public AiReportsApiMappingProfile()
    {
        CreateMap<GetCycleTimeReportRequest, GenerateReportAiInsight.Query>()
            .ForMember(dest => dest.ReportType, opt => opt.MapFrom(_ => ReportType.CycleTime));

        CreateMap<GetBugSeverityDistributionReportRequest, GenerateReportAiInsight.Query>()
            .ForMember(dest => dest.ReportType, opt => opt.MapFrom(_ => ReportType.BugSeverityDistribution));

        CreateMap<GetDeveloperProductivityReportRequest, GenerateReportAiInsight.Query>()
            .ForMember(dest => dest.ReportType, opt => opt.MapFrom(_ => ReportType.DeveloperProductivity));

        CreateMap<GetTeamsProductivityReportRequest, GenerateReportAiInsight.Query>()
            .ForMember(dest => dest.ReportType, opt => opt.MapFrom(_ => ReportType.TeamsProductivity));

        CreateMap<GetBlockedWorkItemsReportRequest, GenerateReportAiInsight.Query>()
            .ForMember(dest => dest.ReportType, opt => opt.MapFrom(_ => ReportType.BlockedWorkItems));

        CreateMap<GetOverdueWorkItemsReportRequest, GenerateReportAiInsight.Query>()
            .ForMember(dest => dest.ReportType, opt => opt.MapFrom(_ => ReportType.OverdueWorkItems));

        CreateMap<GetProjectHealthReportRequest, GenerateReportAiInsight.Query>()
            .ForMember(dest => dest.ReportType, opt => opt.MapFrom(_ => ReportType.ProjectHealth));
    }
}