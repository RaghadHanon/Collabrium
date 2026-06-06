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
    public sealed class RemoveArticleTags
    {
        public sealed class Command : IRequest<Result<ArticleDetailsDto>>
        {
            public required string ArticleId { get; set; }
            public List<string> TagIds { get; set; } = new();
        }

        public sealed class Handler(
            IArticlesRepository articlesRepository,
            IArticlesReadModel articlesReadModel,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<ArticleDetailsDto>>
        {
            public async Task<Result<ArticleDetailsDto>> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<ArticleDetailsDto>.Failure(
                        "Tenant not found.",
                        HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();

                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<ArticleDetailsDto>.Failure(
                        "Current user not found.",
                        HttpStatusCode.Unauthorized);

                var article = await articlesRepository.GetByIdAsync(
                    tenantId,
                    request.ArticleId,
                    cancellationToken);

                if (article is null)
                    return Result<ArticleDetailsDto>.Failure(
                        "Article not found.",
                        HttpStatusCode.NotFound);

                if (!string.Equals(article.CreatedByUserId, currentUserId, StringComparison.Ordinal))
                    return Result<ArticleDetailsDto>.Failure(
                        "You can manage only your own article.",
                        HttpStatusCode.Forbidden);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    foreach (var tagId in request.TagIds)
                    {
                        article.RemoveTag(tagId);
                    }

                    await articlesRepository.RemoveTagsAsync(
                        tenantId,
                        article.Id,
                        request.TagIds,
                        ct);

                }, cancellationToken);

                var dto = await articlesReadModel.GetDetailsAsync(
                    tenantId,
                    article.Id,
                    currentUserId,
                    cancellationToken);

                return dto is null
                    ? Result<ArticleDetailsDto>.Failure(
                        "Article tags removed but article could not be loaded.",
                        HttpStatusCode.InternalServerError)
                    : Result<ArticleDetailsDto>.Success(
                        dto,
                        HttpStatusCode.OK);
            }
        }
    }
}
