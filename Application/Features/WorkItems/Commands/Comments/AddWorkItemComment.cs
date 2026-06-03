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
                return Result<CommentDto>.Failure("Source Work item not found in this project.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<CommentDto>.Failure("Current User not found", HttpStatusCode.NotFound);

            WorkItemComment? comment = null;
            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                comment = new WorkItemComment(currentUserId, request.Content);

                await workItemsRepository.AddCommentOnWorkItemAsync(
                    tenantId,
                    request.ProjectId,
                    request.WorkItemId,
                    comment,
                    cancellationToken);

                historyRecorder.RecordCreationChanges<WorkItemComment>(workItem.ProjectId, workItem.Id, workItem.Title, workItem.Code);
            }, cancellationToken);

            var commentDto = await workItemsReadModel.GetWorkItemComment(request.WorkItemId, comment!.Id, cancellationToken);

            return Result<CommentDto>.Success(commentDto, HttpStatusCode.OK);
        }
    }
}