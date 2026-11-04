using Application.Common;
using Application.Features.Projects.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Projects.Queries;

public class GetUserProjects
{
    public class Query : IRequest<Result<PagedList<ProjectDto, DateTime?>>>
    {
        public required string UserId { get; set; }
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
        IUsersRepository usersRepository,
        ITenantContext tenantContext,
        IMapper mapper
        ) : IRequestHandler<Query, Result<PagedList<ProjectDto, DateTime?>>>
    {
        public async Task<Result<PagedList<ProjectDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<PagedList<ProjectDto, DateTime?>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
                return Result<PagedList<ProjectDto, DateTime?>>.Failure("User not found.", HttpStatusCode.NotFound);

            var parameters = mapper.Map<ProjectsSearchAndFilterParametersDto>(request);

            var list = await projectsReadModel.GetUserProjectsAsync(request.UserId, parameters, cancellationToken);
            return Result<PagedList<ProjectDto, DateTime?>>.Success(list, HttpStatusCode.OK);
        }
    }
}
