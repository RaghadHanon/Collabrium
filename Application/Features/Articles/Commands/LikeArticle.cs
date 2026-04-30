using Application.Common;
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
    public sealed class LikeArticle
    {
        public sealed class Command : IRequest<Result<ArticleDto>>
        {
            public required string ArticleId { get; set; }
        }
        public sealed class Handler(
       IArticlesRepository articlesRepository,
       IArticleLikesRepository articleLikesRepository,
       IArticlesReadModel articlesReadModel,
       ITenantContext tenantContext,
       ICurrentUserAccessor currentUserAccessor,
       IUnitOfWork unitOfWork)
       : IRequestHandler<Command, Result<ArticleDto>>
        {
            public async Task<Result<ArticleDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    return Result<ArticleDto>.Failure("Tenant not found.", System.Net.HttpStatusCode.NotFound);
                }
                var userId = currentUserAccessor.GetUserId();
                if (string.IsNullOrWhiteSpace(userId))
                    return Result<ArticleDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

                var article = await articlesRepository.GetByIdAsync(tenantId, request.ArticleId, cancellationToken);
                if (article is null)
                    return Result<ArticleDto>.Failure("Article not found.", HttpStatusCode.NotFound);
                var exists = await articleLikesRepository.ExistsAsync(request.ArticleId, userId, cancellationToken);
                if (exists)
                    return Result<ArticleDto>.Failure("Article already liked.", HttpStatusCode.BadRequest);
                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    var like = new ArticleLike(request.ArticleId, userId);
                    await articleLikesRepository.AddAsync(like, ct);
                }, cancellationToken);
                var dto = await articlesReadModel.GetByIdAsync(tenantId, article.Id, userId, cancellationToken);
                return dto is null
                    ? Result<ArticleDto>.Failure("Article liked but article could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<ArticleDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
