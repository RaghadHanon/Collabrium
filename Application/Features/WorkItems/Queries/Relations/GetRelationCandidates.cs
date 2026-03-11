using Application.Common;
using Application.Features.WorkItems.DTOs.Relations;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Queries.Relations;

public class GetRelationCandidates
{
    public class Query : IRequest<Result<List<RelationParticipantWorkItemDto>?>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public WorkItemRelationType RelationType { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        ITenantContext tenantContext,
        IWorkItemsReadModel workItemsReadModel,
        IProjectsRepository projectsRepository
    ) : IRequestHandler<Query, Result<List<RelationParticipantWorkItemDto>?>>
    {
        public async Task<Result<List<RelationParticipantWorkItemDto>?>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<List<RelationParticipantWorkItemDto>?>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<List<RelationParticipantWorkItemDto>?>.Failure("Project not found.", HttpStatusCode.NotFound);

            var sourceWorkItem = await workItemsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.WorkItemId,
                includeChildren: true,
                cancellationToken);

            if (sourceWorkItem is null)
                return Result<List<RelationParticipantWorkItemDto>?>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            var dto = await workItemsReadModel.GetRelationCandidates(
                    tenantId,
                    request.ProjectId,
                    request.WorkItemId,
                    request.RelationType,
                    cancellationToken);

            return Result<List<RelationParticipantWorkItemDto>?>.Success(dto, HttpStatusCode.OK);
        }
    }
}