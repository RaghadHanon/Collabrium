using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands.Comments;

public class UpdateWorkItemComment
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public required string CommentId { get; set; }
        public required string NewContent { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        IProjectHistoryRecorder historyRecorder,
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

            var workItem = await workItemsRepository.GetByIdAsync(tenantId, request.ProjectId, request.WorkItemId, false, cancellationToken);
            if (workItem is null)
                return Result<Unit>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<Unit>.Failure("Current User not found.", HttpStatusCode.NotFound);

            var comment = await workItemsRepository.GetCommentOnWorkItemByIdAsync(request.WorkItemId, request.CommentId, cancellationToken);
            if (comment == null)
                return Result<Unit>.Failure("Comment not found.", HttpStatusCode.NotFound);

            if (currentUserId != comment.CreatedByUserId)
                return Result<Unit>.Failure("You don't own this comment to update it.", HttpStatusCode.Forbidden);

            var oldCommentState = comment.Clone();
            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                comment.Edit(request.NewContent);

                await workItemsRepository.UpdateCommentOfWorkItemAsync(
                    request.WorkItemId,
                    comment,
                    cancellationToken);

                historyRecorder.RecordUpdatingChanges<Comment>(request.ProjectId, workItem.Id, workItem.Title, oldCommentState, comment);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}