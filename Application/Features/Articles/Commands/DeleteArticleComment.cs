using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Articles.Commands
{
    public sealed class DeleteArticleComment
    {
        public sealed class Command : IRequest<Result<string>>
        {
            public required string ArticleId { get; set; }
            public required string CommentId { get; set; }
        }

        public sealed class Handler(
            IArticleCommentsRepository commentsRepository,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<string>>
        {
            public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var userId = currentUserAccessor.GetUserId();
                if (string.IsNullOrWhiteSpace(userId))
                    return Result<string>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

                var comment = await commentsRepository.GetByIdAsync(
                    request.ArticleId,
                    request.CommentId,
                    cancellationToken);

                if (comment is null)
                    return Result<string>.Failure("Comment not found.", HttpStatusCode.NotFound);

                if (!string.Equals(comment.CreatedByUserId, userId, StringComparison.Ordinal))
                    return Result<string>.Failure("You can delete only your own comment.", HttpStatusCode.Forbidden);

                var removed = false;

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    removed = await commentsRepository.RemoveAsync(
                        request.ArticleId,
                        request.CommentId,
                        ct);
                }, cancellationToken);

                return !removed
                    ? Result<string>.Failure("Comment not found.", HttpStatusCode.NotFound)
                    : Result<string>.Success("Comment deleted successfully.", HttpStatusCode.OK);
            }
        }
    }
}



