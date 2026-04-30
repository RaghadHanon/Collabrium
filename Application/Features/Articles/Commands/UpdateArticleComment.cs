using Application.Common;
using Application.Features.Articles.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Articles.Commands
{
    public sealed class Command : IRequest<Result<ArticleCommentDto>>
    {
        public required string ArticleId { get; set; }
        public required string CommentId { get; set; }
        public required string Content { get; set; }
    }
    public sealed class Handler(
       IArticleCommentsRepository commentsRepository,
       IArticleCommentsReadModel commentsReadModel,
       ITenantContext tenantContext,
       ICurrentUserAccessor currentUserAccessor,
       IUnitOfWork unitOfWork)
       : IRequestHandler<Command, Result<ArticleCommentDto>>
    {
        public async Task<Result<ArticleCommentDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<ArticleCommentDto>.Failure("Tenant not found.", System.Net.HttpStatusCode.NotFound);
            var userId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Result<ArticleCommentDto>.Failure("Current user not found.", System.Net.HttpStatusCode.Unauthorized);
            var comment = await commentsRepository.GetByIdAsync(tenantId, request.CommentId, cancellationToken);
            if (comment is null)
                return Result<ArticleCommentDto>.Failure("Comment not found.", HttpStatusCode.NotFound);
            if (!string.Equals(comment.CreatedByUserId, userId, StringComparison.Ordinal))
                return Result<ArticleCommentDto>.Failure("You can update only your own comment.", HttpStatusCode.Forbidden);
            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                comment.UpdateContent(request.Content);
                await commentsRepository.UpdateAsync(comment, ct);

            }, cancellationToken);
            var dto = await commentsReadModel.GetByIdAsync(tenantId, comment.Id, cancellationToken);
            return dto is null
                ? Result<ArticleCommentDto>.Failure("Article comment updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<ArticleCommentDto>.Success(dto, HttpStatusCode.OK);

        }
    }
}


