using Application.Common;
using Application.Features.Teams.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Teams.Queries;

public class GetTeams
{
    public class Query : IRequest<Result<PagedList<TeamDto, DateTime?>>>
    {
        public string? TeamLeadName { get; set; }
        public string? NameContains { get; set; }
        public string? DescriptionContains { get; set; }
        public Department? Department { get; set; }
        public bool? IsActive { get; set; }
        public int? CapacityMin { get; set; }
        public int? CapacityMax { get; set; }
        public DateTime? UpdatedAfter { get; set; }
        public DateTime? UpdatedBefore { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? Cursor { get; set; }
        public int PageSize { get; set; }
    }

    public class Handler(
        ITeamsRepository teamsRepository,
        ITenantContext tenantContext,
        IMapper mapper
    ) : IRequestHandler<Query, Result<PagedList<TeamDto, DateTime?>>>
    {
        public async Task<Result<PagedList<TeamDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenant = tenantContext.CurrentTenant;
            if (tenant is null)
            {
                return Result<PagedList<TeamDto, DateTime?>>.Failure("Tenant not found.",
                    HttpStatusCode.NotFound);
            }

            var queryParams = mapper.Map<TeamsSearchAndFilterParametersDto>(request);
            queryParams.TenantId = tenant.Id;
            var pagedTeams = await teamsRepository.GetTeamsAsync(queryParams, cancellationToken);
            var teamDtos = pagedTeams.Items.Select(t => mapper.Map<TeamDto>(t)).ToList();

            return Result<PagedList<TeamDto, DateTime?>>.Success(
                new PagedList<TeamDto, DateTime?>
                {
                    Items = teamDtos,
                    NextCursor = pagedTeams.NextCursor
                },
                HttpStatusCode.OK
            );
        }
    }
}
