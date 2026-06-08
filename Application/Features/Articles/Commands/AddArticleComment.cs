using Application.Common;
using Application.Events;
using Application.Features.Articles.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Articles.Commands
{
    public sealed class AddArticleComment
    {
        public sealed class Command : IRequest<Result<ArticleCommentDto>>
        {
            public required string ArticleId { get; set; }
            public required string Content { get; set; }
        }
        public sealed class Handler(
           IArticlesRepository articlesRepository,
           IArticleCommentsRepository commentsRepository,
           IArticleCommentsReadModel commentsReadModel,
           ITenantContext tenantContext,
           IUsersReadModel usersReadModel,
           IMediator mediator,
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
                var article = await articlesRepository.GetByIdAsync(tenantId, request.ArticleId, cancellationToken);
                if (article is null)
                    return Result<ArticleCommentDto>.Failure("Article not found.", HttpStatusCode.NotFound);
                ArticleComment comment = null!;
                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    comment = new ArticleComment(
                   articleId: request.ArticleId,
                   content: request.Content,
                   createdByUserId: userId);
                    await commentsRepository.AddAsync(comment, ct);
                }, cancellationToken);
                var dto = await commentsReadModel.GetByIdAsync(
                      request.ArticleId,
                      comment.Id,
                      cancellationToken);
                var commenter = await usersReadModel.GetUserSnapshotByIdAsync(
                    userId,
                    cancellationToken);

                if (commenter is not null && article.CreatedByUserId != userId)
                {
                    await mediator.Publish(new ArticleCommentAddedEvent(
                        TenantId: tenantId,
                        ArticleId: article.Id,
                        ArticleTitle: article.Title,
                        ArticleOwnerUserId: article.CreatedByUserId,
                        CommentId: comment.Id,
                        CommenterUserId: userId,
                        CommenterFullName: commenter.FullName
                    ), cancellationToken);
                }
                return dto is null
                    ? Result<ArticleCommentDto>.Failure("Article comment added but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<ArticleCommentDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
