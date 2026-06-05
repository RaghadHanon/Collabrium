using Application.Common;
using Application.Events;
using Application.Features.WorkItems.DTOs.Comments;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands.Comments;

public class AddWorkItemComment
{
    public class Command : IRequest<Result<CommentDto>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public required string Content { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        IWorkItemsReadModel workItemsReadModel,
        IProjectHistoryRecorder historyRecorder,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor,
        IProjectsRepository projectsRepository,
        IMediator mediator
    ) : IRequestHandler<Command, Result<CommentDto>>
    {
        public async Task<Result<CommentDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id ?? tenantContext.TenantId;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<CommentDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(
                    tenantId,
                    request.ProjectId,
                    cancellationToken))
            {
                return Result<CommentDto>.Failure(
                    "Project not found.",
                    HttpStatusCode.NotFound);
            }

            var workItem = await workItemsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.WorkItemId,
                false,
                cancellationToken);

            if (workItem is null)
                return Result<CommentDto>.Failure(
                    "Source Work item not found in this project.",
                    HttpStatusCode.NotFound);

            var currentUser = await currentUserAccessor.GetCurrentUserAsync(cancellationToken);
            if (currentUser is null)
                return Result<CommentDto>.Failure(
                    "Current User not found",
                    HttpStatusCode.NotFound);

            WorkItemComment? comment = null;

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                comment = new WorkItemComment(currentUser.Id, request.Content);

                await workItemsRepository.AddCommentOnWorkItemAsync(
                    tenantId,
                    request.ProjectId,
                    request.WorkItemId,
                    comment,
                    ct);

                historyRecorder.RecordCreationChanges<WorkItemComment>(
                    workItem.ProjectId,
                    workItem.Id,
                    workItem.Title,
                    workItem.Code);

            }, cancellationToken);

            if (comment is null)
                return Result<CommentDto>.Failure(
                    "Comment could not be created.",
                    HttpStatusCode.InternalServerError);

            var commentDto = await workItemsReadModel.GetWorkItemComment(
                request.WorkItemId,
                comment.Id,
                cancellationToken);

            if (commentDto is null)
                return Result<CommentDto>.Failure(
                    "Comment created but could not be loaded.",
                    HttpStatusCode.InternalServerError);

            var recipientUserIds = new[]
                {
                    workItem.AssignedToUserId,
                    workItem.CreatedByUserId
                }
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id!.Trim())
                .Where(id => !string.Equals(id, currentUser.Id, StringComparison.Ordinal))
                .Distinct()
                .ToList();

            if (recipientUserIds.Count > 0)
            {
                await mediator.Publish(new WorkItemCommentAddedEvent(
                        tenantId,
                        request.ProjectId,
                        workItem.Id,
                        workItem.Title,
                        workItem.Code,
                        workItem.SprintId,
                        comment.Id,
                        currentUser.Id,
                        currentUser.FullName,
                        recipientUserIds
                    ), cancellationToken);
            }

            return Result<CommentDto>.Success(commentDto, HttpStatusCode.OK);
        }
    }
}