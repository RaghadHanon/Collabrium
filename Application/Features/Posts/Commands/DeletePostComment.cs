using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Posts.Commands
{
    public sealed class DeletePostComment
    {
        public sealed class Command : IRequest<Result<Unit>>
        {
            public required string PostId { get; set; }
            public required string CommentId { get; set; }
        }

        public sealed class Handler(
            IPostsRepository postsRepository,
            IPostCommentsRepository postCommentsRepository,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();

                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<Unit>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

                var postExists = await postsRepository.ExistsAsync(
                    tenantId,
                    request.PostId,
                    cancellationToken);

                if (!postExists)
                    return Result<Unit>.Failure("Post not found.", HttpStatusCode.NotFound);

                var comment = await postCommentsRepository.GetByIdAsync(
                    request.PostId,
                    request.CommentId,
                    cancellationToken);

                if (comment is null)
                    return Result<Unit>.Failure("Comment not found.", HttpStatusCode.NotFound);

                if (!string.Equals(comment.CreatedByUserId, currentUserId, StringComparison.Ordinal))
                    return Result<Unit>.Failure(
                        "You can delete only your own comment.",
                        HttpStatusCode.Forbidden);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    await postCommentsRepository.DeleteAsync(
                        request.PostId,
                        request.CommentId,
                        ct);

                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                return Result<Unit>.Success(Unit.Value, HttpStatusCode.OK);
            }
        }
    }
}
