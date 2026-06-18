using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Features.Articles.Commands
{
    public sealed class UploadArticleCoverImage
    {
        public sealed class Command : IRequest<Result<Unit>>
        {
            public required string ArticleId { get; set; }
            public required IFormFile File { get; set; }
        }

        public sealed class Handler(
            IAttachmentStorageFactory storageFactory,
            IArticlesRepository articlesRepository,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                var currentUserId = currentUserAccessor.GetUserId();
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                if (request.File is null || request.File.Length == 0)
                    return Result<Unit>.Failure("No file uploaded.", HttpStatusCode.BadRequest);

                var article = await articlesRepository.GetByIdAsync(
                    tenantId,
                    request.ArticleId,
                    cancellationToken);

                if (article is null)
                    return Result<Unit>.Failure("Article not found.", HttpStatusCode.NotFound);
                if (!string.Equals(article.CreatedByUserId, currentUserId, StringComparison.Ordinal))
                    return Result<Unit>.Failure(
                        "You can manage only your own article.",
                        HttpStatusCode.Forbidden);

                var storage = storageFactory.Get(AttachmentType.Image);

                var stored = await storage.UploadAsync(
                    request.File,
                    AttachmentType.Image,
                    folder: "Collabrium/ArticleCoverImages",
                    ct: cancellationToken);

                if (stored is null)
                    return Result<Unit>.Failure("Failed to upload the cover image.", HttpStatusCode.BadRequest);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    article.SetCoverImage(stored.Url, stored.StoragePath);

                    await articlesRepository.UpdateAsync(article, ct);
                    await unitOfWork.SaveChangesAsync(ct);

                }, cancellationToken);

                return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
            }
        }
    }
}
