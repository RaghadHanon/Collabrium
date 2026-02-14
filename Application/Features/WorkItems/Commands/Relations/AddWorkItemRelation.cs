using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands.Relations;

public class AddWorkItemRelation
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string ProjectId { get; set; }
        public required string SourceWorkItemId { get; set; }
        public required string TargetWorkItemId { get; set; }
        public required WorkItemRelationType RelationType { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor,
        IProjectsRepository projectsRepository
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<Unit>.Failure("Project not found.", HttpStatusCode.NotFound);

            var sourceWorkItem = await workItemsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.SourceWorkItemId,
                includeChildren: true,
                cancellationToken);

            if (sourceWorkItem is null)
                return Result<Unit>.Failure("Source Work item not found in this project.", HttpStatusCode.NotFound);

            var targetWorkItem = await workItemsRepository.GetByIdAsync(
               tenantId,
               request.ProjectId,
               request.TargetWorkItemId,
               includeChildren: true,
               cancellationToken);

            if (targetWorkItem is null)
                return Result<Unit>.Failure("Target Work item not found in this project.", HttpStatusCode.NotFound);

            var userRole = await currentUserAccessor.GetUserRole(currentUserAccessor.GetUserId()!, cancellationToken);
            var isAdmin = Roles.ProjectsManagerRoles.Contains(userRole);

            if ((sourceWorkItem.Type == WorkItemType.Epic || sourceWorkItem.Type == WorkItemType.UserStory)
                && !isAdmin)
                return Result<Unit>.Failure("Only admins can change relations of Epics and User Stories.", HttpStatusCode.Forbidden);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await workItemsRepository.PersistRelationAsync(
                    tenantId,
                    request.ProjectId,
                    request.SourceWorkItemId,
                    request.TargetWorkItemId,
                    request.RelationType,
                    ct);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}