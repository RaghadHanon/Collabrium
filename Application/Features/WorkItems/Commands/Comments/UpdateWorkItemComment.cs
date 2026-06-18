using Application.Common;
using Application.Features.WorkItems.DTOs.Comments;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands.Comments;

public class UpdateWorkItemComment
{
    public class Command : IRequest<Result<CommentDto>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public required string CommentId { get; set; }
        public required string Content { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        IWorkItemsReadModel workItemsReadModel,
        IProjectHistoryRecorder historyRecorder,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor,
        IProjectsRepository projectsRepository
    ) : IRequestHandler<Command, Result<CommentDto>>
    {
        public async Task<Result<CommentDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<CommentDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<CommentDto>.Failure("Project not found.", HttpStatusCode.NotFound);

            var workItem = await workItemsRepository.GetByIdAsync(tenantId, request.ProjectId, request.WorkItemId, false, cancellationToken);
            if (workItem is null)
                return Result<CommentDto>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<CommentDto>.Failure("Current User not found.", HttpStatusCode.NotFound);

            var comment = await workItemsRepository.GetCommentOnWorkItemByIdAsync(request.WorkItemId, request.CommentId, cancellationToken);
            if (comment == null)
                return Result<CommentDto>.Failure("Comment not found.", HttpStatusCode.NotFound);

            if (currentUserId != comment.CreatedByUserId)
                return Result<CommentDto>.Failure("You don't own this comment to update it.", HttpStatusCode.Forbidden);

            var oldCommentState = comment.Clone();
            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                comment.Edit(request.Content);

                await workItemsRepository.UpdateCommentOfWorkItemAsync(
                    request.WorkItemId,
                    comment,
                    cancellationToken);

                await historyRecorder.RecordUpdatingChangesAsync(
                    projectId: request.ProjectId,
                    relatedEntityId: workItem.Id,
                    relatedEntityTitle: workItem.Title,
                    relatedEntityKey: workItem.Code,
                    oldObject: oldCommentState,
                    newObject: comment,
                    cancellationToken: cancellationToken);
            }, cancellationToken);

            var commentDto = await workItemsReadModel.GetWorkItemComment(request.WorkItemId, comment.Id, cancellationToken);

            return Result<CommentDto>.Success(commentDto, HttpStatusCode.NoContent);
        }
    }
}