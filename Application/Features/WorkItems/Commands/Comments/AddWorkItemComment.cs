using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands.Comments;

public class AddWorkItemComment
{
    public class Command : IRequest<Result<string>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public required string Content { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor,
        IProjectsRepository projectsRepository
    ) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<string>.Failure("Project not found.", HttpStatusCode.NotFound);

            var isExist = await workItemsRepository.IsExistAsync(tenantId, request.ProjectId, request.WorkItemId, cancellationToken);
            if (!isExist)
                return Result<string>.Failure("Source Work item not found in this project.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<string>.Failure("Current User not found", HttpStatusCode.NotFound);

            Comment? comment = null;
            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                comment = new Comment(currentUserId, request.Content);

                await workItemsRepository.AddCommentOnWorkItemAsync(
                    tenantId,
                    request.ProjectId,
                    request.WorkItemId,
                    comment,
                    cancellationToken);
            }, cancellationToken);

            return Result<string>.Success(comment?.Id, HttpStatusCode.OK);
        }
    }
}