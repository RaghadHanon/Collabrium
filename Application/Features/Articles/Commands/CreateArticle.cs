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
    public sealed class CreateArticle
    {

        public sealed class Command : IRequest<Result<ArticleDetailsDto>>
        {
            public required string Title { get; set; }
            public string? Description { get; set; }
            public required string Content { get; set; }
            public bool IsPublished { get; set; } = true;
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
            public async Task<Result<ArticleDetailsDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<ArticleDetailsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();

                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<ArticleDetailsDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

                Article article = null!;

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    article = new Article(
                        tenantId: tenantId,
                        title: request.Title,
                        description: request.Description,
                        content: request.Content,
                        createdByUserId: currentUserId,
                        isPublished: request.IsPublished);

                    await articlesRepository.AddAsync(article, ct);
                    await unitOfWork.SaveChangesAsync(ct);

                    if (request.TagIds is { Count: > 0 })
                    {
                        article.SetTags(request.TagIds);

                        await articlesRepository.SetTagsAsync(
                            tenantId,
                            article.Id,
                            request.TagIds,
                            ct);
                    }

                }, cancellationToken);

                var dto = await articlesReadModel.GetDetailsAsync(
                    tenantId,
                    article.Id,
                    currentUserId,
                    cancellationToken);

                return dto is null
                    ? Result<ArticleDetailsDto>.Failure("Article created but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<ArticleDetailsDto>.Success(dto, HttpStatusCode.Created);
            }
        }
    }
}

