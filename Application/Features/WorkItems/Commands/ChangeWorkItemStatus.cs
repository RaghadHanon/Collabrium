using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands;

public sealed class ChangeWorkItemStatus
{
    public sealed class Command : IRequest<Result<WorkItemDto>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public required WorkItemStatus NewStatus { get; set; }
    }

    public sealed class Handler(
        IWorkItemsRepository workItemsRepository,
        IWorkItemsReadModel workItemsReadModel,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor
    ) : IRequestHandler<Command, Result<WorkItemDto>>
    {
        public async Task<Result<WorkItemDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<WorkItemDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var workItem = await workItemsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.WorkItemId,
                includeChildren: true,
                cancellationToken);

            if (workItem is null)
                return Result<WorkItemDto>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            var userRole = await currentUserAccessor.GetUserRole(currentUserAccessor.GetUserId()!, cancellationToken);
            var isAdmin = Roles.ProjectsManagerRoles.Contains(userRole);

            if ((workItem.Type == WorkItemType.Epic || workItem.Type == WorkItemType.UserStory)
                && !isAdmin)
                return Result<WorkItemDto>.Failure("Only admins can change the status of Epics and User Stories.", HttpStatusCode.Forbidden);

            var allowedStatuses = workItem.GetAllowedStatuses();
            if (!allowedStatuses.Contains(request.NewStatus))
                return Result<WorkItemDto>.Failure($"Invalid status transition. Allowed statuses: {string.Join(", ", allowedStatuses)}.", HttpStatusCode.BadRequest);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                workItem.ChangeStatus(request.NewStatus);

                await workItemsRepository.UpdateAsync(workItem, ct);
            }, cancellationToken);

            var dto = await workItemsReadModel.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.WorkItemId,
                cancellationToken);

            return dto is null
                ? Result<WorkItemDto>.Failure("Work item updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<WorkItemDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}