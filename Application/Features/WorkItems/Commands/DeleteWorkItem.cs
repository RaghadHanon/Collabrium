using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands;

public sealed class DeleteWorkItem
{
    public sealed class Command : IRequest<Result<Unit>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
    }

    public sealed class Handler(
        IWorkItemsRepository workItemsRepository,
        IProjectHistoryRecorder historyRecorder,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        IProjectsRepository projectsRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<Unit>.Failure("Project not found.", HttpStatusCode.NotFound);

            var workItem = await workItemsRepository.GetByIdAsync(tenantId, request.ProjectId, request.WorkItemId, false, cancellationToken);
            if (workItem == null)
                return Result<Unit>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            var userRole = await currentUserAccessor.GetUserRole(currentUserAccessor.GetUserId()!, cancellationToken);
            var isAdmin = Roles.ProjectsManagerRoles.Contains(userRole);

            if ((workItem.Type == WorkItemType.Epic || workItem.Type == WorkItemType.UserStory)
                && !isAdmin)
                return Result<Unit>.Failure("Only admins can change the status of Epics and User Stories.", HttpStatusCode.Forbidden);

            var removed = false;

            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                removed = await workItemsRepository.RemoveAsync(tenantId, request.ProjectId, request.WorkItemId, cancellationToken);

                historyRecorder.RecordDeletionChanges<WorkItem>(workItem.ProjectId, workItem.Id, workItem.Title);
            }, cancellationToken);

            return removed
                ? Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent)
                : Result<Unit>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);
        }
    }
}