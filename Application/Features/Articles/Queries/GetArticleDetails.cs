using Application.Common;
using Application.Features.Articles.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Articles.Queries
{
    public sealed class GetArticleDetails
    {
        public sealed class Query : IRequest<Result<ArticleDetailsDto>>
        {
            public required string ArticleId { get; set; }
        }

        public sealed class Handler(
           IArticlesReadModel articlesReadModel,
           IArticlesRepository articlesRepository,
           ITenantContext tenantContext,
           ICurrentUserAccessor currentUserAccessor)
           : IRequestHandler<Query, Result<ArticleDetailsDto>>
        {
            public async Task<Result<ArticleDetailsDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<ArticleDetailsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var article = await articlesRepository.GetByIdAsync(
                    tenantId,
                    request.ArticleId,
                    cancellationToken);

                if (article is null)
                    return Result<ArticleDetailsDto>.Failure("Article not found.", HttpStatusCode.NotFound);

                var canViewHidden = await RoadmapAccessHelper.CanCurrentUserViewHiddenAsync(
                    currentUserAccessor,
                    cancellationToken);

                if (!article.IsPublished && !canViewHidden)
                    return Result<ArticleDetailsDto>.Failure("Article not found.", HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();

                var dto = await articlesReadModel.GetDetailsAsync(
                    tenantId,
                    request.ArticleId,
                    currentUserId,
                    cancellationToken);

                return dto is null
                    ? Result<ArticleDetailsDto>.Failure("Article not found.", HttpStatusCode.NotFound)
                    : Result<ArticleDetailsDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}


