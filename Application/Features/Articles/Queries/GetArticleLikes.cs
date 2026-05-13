using Application.Common;
using Application.Features.Articles.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Articles.Queries
{
    public sealed class GetArticleLikes
    {
        public sealed class Query : IRequest<Result<PagedList<ArticleLikeUserDto, DateTime?>>>
        {
            public required string ArticleId { get; set; }
            public int PageSize { get; set; } = 10;
            public DateTime? Cursor { get; set; }
        }

        public sealed class Handler(
            IArticlesRepository articlesRepository,
            IArticlesReadModel articlesReadModel,
            ITenantContext tenantContext)
            : IRequestHandler<Query, Result<PagedList<ArticleLikeUserDto, DateTime?>>>
        {
            public async Task<Result<PagedList<ArticleLikeUserDto, DateTime?>>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PagedList<ArticleLikeUserDto, DateTime?>>.Failure(
                        "Tenant not found.",
                        HttpStatusCode.NotFound);

                var article = await articlesRepository.GetByIdAsync(
                    tenantId,
                    request.ArticleId,
                    cancellationToken);

                if (article is null)
                    return Result<PagedList<ArticleLikeUserDto, DateTime?>>.Failure(
                        "Article not found.",
                        HttpStatusCode.NotFound);

                var result = await articlesReadModel.GetArticleLikesAsync(
                    tenantId,
                    request.ArticleId,
                    request.PageSize,
                    request.Cursor,
                    cancellationToken);

                return Result<PagedList<ArticleLikeUserDto, DateTime?>>.Success(
                    result,
                    HttpStatusCode.OK);
            }
        }
    }
}
