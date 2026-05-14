using Application.Common;
using Application.Features.ProjectsHistories.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.ProjectsHistories.Queries;
public class GetProjectHistory
{
    public class Query : IRequest<Result<PagedList<ProjectHistoryDto, DateTime?>>>
    {
        public required string ProjectId { get; set; }
        public string? MadeByUserId { get; set; }
        public string? PropertyName { get; set; }
        public ProjectHistoryScope? Scope { get; set; }
        public ActionType? ActionType { get; set; }
        public string? RelatedEntityTitle { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

        public DateTime? Cursor { get; set; }
        public int PageSize { get; set; }
    }

    public class Handler(
        IProjectHistoriesReadModel historyReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        IProjectsRepository projectsRepository,
        IMapper mapper)
        : IRequestHandler<Query, Result<PagedList<ProjectHistoryDto, DateTime?>>>
    {
        public async Task<Result<PagedList<ProjectHistoryDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<PagedList<ProjectHistoryDto, DateTime?>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<PagedList<ProjectHistoryDto, DateTime?>>.Failure("Project not found.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<PagedList<ProjectHistoryDto, DateTime?>>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

            var parameters = mapper.Map<ProjectHistorySearchAndFiltersParametersDto>(request);
            var historyEntries = await historyReadModel.GetPagedListAsync(parameters, request.ProjectId, tenantId, cancellationToken);

            return Result<PagedList<ProjectHistoryDto, DateTime?>>.Success(historyEntries, HttpStatusCode.OK);
        }
    }
}
