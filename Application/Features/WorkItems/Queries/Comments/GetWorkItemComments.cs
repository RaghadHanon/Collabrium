using Application.Common;
using Application.Features.WorkItems.DTOs.Comments;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Queries.Comments;

public class GetWorkItemComments
{
    public class Query : IRequest<Result<PagedList<CommentDto, DateTime?>>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public int PageSize { get; set; }
        public DateTime? Cursor { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        ITenantContext tenantContext,
        IWorkItemsReadModel workItemsReadModel,
        IProjectsRepository projectsRepository,
        IMapper mapper
    ) : IRequestHandler<Query, Result<PagedList<CommentDto, DateTime?>>>
    {
        public async Task<Result<PagedList<CommentDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<PagedList<CommentDto, DateTime?>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<PagedList<CommentDto, DateTime?>>.Failure("Project not found.", HttpStatusCode.NotFound);

            var isExist = await workItemsRepository.IsExistAsync(tenantId, request.ProjectId, request.WorkItemId, cancellationToken);
            if (!isExist)
                return Result<PagedList<CommentDto, DateTime?>>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            var parameters = mapper.Map<CommentsQueryParametersDto>(request);
            var dtoList = await workItemsReadModel.GetWorkItemComments(parameters, request.WorkItemId, cancellationToken);

            return Result<PagedList<CommentDto, DateTime?>>.Success(dtoList, HttpStatusCode.OK);
        }
    }
}