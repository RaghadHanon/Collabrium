using Application.Common;
using Application.Features.WorkItems.DTOs.Relations;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Queries.Relations;

public class GetWorkItemRelation
{
    public class Query : IRequest<Result<WorkItemRelationDto>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public required string RelationId { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        ITenantContext tenantContext,
        IWorkItemsReadModel workItemsReadModel,
        IProjectsRepository projectsRepository
    ) : IRequestHandler<Query, Result<WorkItemRelationDto>>
    {
        public async Task<Result<WorkItemRelationDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<WorkItemRelationDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<WorkItemRelationDto>.Failure("Project not found.", HttpStatusCode.NotFound);

            var sourceWorkItem = await workItemsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.WorkItemId,
                includeChildren: true,
                cancellationToken);

            if (sourceWorkItem is null)
                return Result<WorkItemRelationDto>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            var dto = await workItemsReadModel.GetRelationByIdAsync(
                    request.ProjectId,
                    request.WorkItemId,
                    request.RelationId,
                    cancellationToken);

            return dto is null
                ? Result<WorkItemRelationDto>.Failure("Work item not can't be loaded or does not belong to this workItem.", HttpStatusCode.InternalServerError)
                : Result<WorkItemRelationDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}