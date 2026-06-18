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

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
            { 
                return Result<Unit>.Failure("Current user not found.", HttpStatusCode.Unauthorized); 
            }

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<Unit>.Failure("Project not found.", HttpStatusCode.NotFound);

            var workItem = await workItemsRepository.GetByIdAsync(tenantId, request.ProjectId, request.WorkItemId, false, cancellationToken);
            if (workItem == null)
                return Result<Unit>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            var userRole = await currentUserAccessor.GetUserRole(currentUserId, cancellationToken); 
            var isAdmin = Roles.ProjectsManagerRoles.Contains(userRole); 
            var isHighLevelWorkItem = workItem.Type is WorkItemType.Epic or WorkItemType.UserStory; 

            if (isHighLevelWorkItem && !isAdmin) 
            {
                return Result<Unit>.Failure( "Only admins can delete Epics and User Stories.", HttpStatusCode.Forbidden);
            }

            var isCreator = string.Equals( workItem.CreatedByUserId, currentUserId, StringComparison.Ordinal); 
            if (!isAdmin && !isCreator)
            {
                return Result<Unit>.Failure( "You can only delete work items that you created.", HttpStatusCode.Forbidden); 
            }

            var removed = false;
            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                removed = await workItemsRepository.RemoveAsync(tenantId, request.ProjectId, request.WorkItemId, cancellationToken);

                historyRecorder.RecordDeletionChanges<WorkItem>(workItem.ProjectId, workItem.Id, workItem.Title, workItem.Code);
            }, cancellationToken);

            return removed
                ? Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent)
                : Result<Unit>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);
        }
    }
}