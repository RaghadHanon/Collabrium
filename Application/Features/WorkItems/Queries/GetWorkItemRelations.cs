using Application.Common;
using Application.Features.WorkItems.DTOs.Relations;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Queries;

public class GetWorkItemRelations
{
    public class Command : IRequest<Result<List<WorkItemRelationDto>?>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public WorkItemRelationType? RelationType { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        ITenantContext tenantContext,
        IWorkItemsReadModel workItemsReadModel,
        IProjectsRepository projectsRepository
    ) : IRequestHandler<Command, Result<List<WorkItemRelationDto>?>>
    {
        public async Task<Result<List<WorkItemRelationDto>?>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<List<WorkItemRelationDto>?>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<List<WorkItemRelationDto>?>.Failure("Project not found.", HttpStatusCode.NotFound);

            var sourceWorkItem = await workItemsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.WorkItemId,
                includeChildren: true,
                cancellationToken);

            if (sourceWorkItem is null)
                return Result<List<WorkItemRelationDto>?>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            var dto = await workItemsReadModel.GetRelationsByTypeAsync(
                    tenantId,
                    request.ProjectId,
                    request.WorkItemId,
                    request.RelationType,
                    cancellationToken);

            return Result<List<WorkItemRelationDto>?>.Success(dto, HttpStatusCode.OK);
        }
    }
}