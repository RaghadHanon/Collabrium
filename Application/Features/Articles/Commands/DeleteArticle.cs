using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Articles.Commands
{
    public sealed class DeleteArticle
    {
        public sealed class Command : IRequest<Result<string>>
        {
            public string ArticleId { get; set; } = null!;
        }
        public sealed class Handler(
       IArticlesRepository articlesRepository,
       ITenantContext tenantContext,
       ICurrentUserAccessor currentUserAccessor,
       IUnitOfWork unitOfWork)
       : IRequestHandler<Command, Result<string>>
        {
            public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    return Result<string>.Failure("Tenant context is missing.", HttpStatusCode.NotFound);
                }
                var article = await articlesRepository.GetByIdAsync(tenantId, request.ArticleId, cancellationToken);
                if (article is null)
                {
                    return Result<string>.Failure("Article not found.", HttpStatusCode.NotFound);
                }
                var currentUserId = currentUserAccessor.GetUserId();
                if (!string.Equals(article.CreatedByUserId, currentUserId, StringComparison.Ordinal))
                    return Result<string>.Failure(
                        "You can manage only your own article.",
                        HttpStatusCode.Forbidden);

                var removed = false;
                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    removed = await articlesRepository.RemoveAsync(tenantId, request.ArticleId, ct);
                }, cancellationToken);
                return !removed
                ? Result<string>.Failure("Article not found.", HttpStatusCode.NotFound)
                : Result<string>.Success("Article deleted successfully.", HttpStatusCode.OK);
            }
        }
    }
}
