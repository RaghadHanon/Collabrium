using Application.Common;
using Application.Features.Projects.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Projects.Queries;

public class GetProjects
{
    public class Query : IRequest<Result<PagedList<ProjectDto, DateTime?>>>
    {
        public string? TeamId { get; set; }
        public string? NameContains { get; set; }
        public bool AssignedToTeam { get; set; }
        public string? Key { get; set; }

        public ProjectStatus? Status { get; set; }

        public DateTime? StartAfter { get; set; }
        public DateTime? StartBefore { get; set; }

        public DateTime? EndAfter { get; set; }
        public DateTime? EndBefore { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? Cursor { get; set; }
        public int PageSize { get; set; }
    }

    public class Handler(
        IProjectsReadModel projectsReadModel,
        IMapper mapper
        ) : IRequestHandler<Query, Result<PagedList<ProjectDto, DateTime?>>>
    {
        public async Task<Result<PagedList<ProjectDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = mapper.Map<ProjectsSearchAndFilterParametersDto>(request);

            var list = await projectsReadModel.GetProjectsAsync(parameters, cancellationToken);
            return Result<PagedList<ProjectDto, DateTime?>>.Success(list, HttpStatusCode.OK);
        }
    }
}