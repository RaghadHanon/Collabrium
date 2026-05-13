using Application.Common;
using Application.Features.Posts.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Posts.Commands
{
    public sealed class RemovePostImage
    {
        public sealed class Command : IRequest<Result<PostDto>>
        {
            public required string PostId { get; set; }
            public required string StoragePath { get; set; }
        }

        public sealed class Handler(
            IPostsRepository postsRepository,
            IPostsReadModel postsReadModel,
            IAttachmentStorageFactory storageFactory,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<PostDto>>
        {
            public async Task<Result<PostDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PostDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var post = await postsRepository.GetByIdAsync(tenantId, request.PostId, cancellationToken);
                if (post is null)
                    return Result<PostDto>.Failure("Post not found.", HttpStatusCode.NotFound);
                var currentUserId = currentUserAccessor.GetUserId();
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<PostDto>.Failure(
                        "Current user not found.",
                        HttpStatusCode.Unauthorized);

                if (!string.Equals(post.CreatedByUserId, currentUserId, StringComparison.Ordinal))
                    return Result<PostDto>.Failure(
                        "You can remove images only from your own post.",
                        HttpStatusCode.Forbidden);
                var storagePath = request.StoragePath.Trim();

                if (!post.Images.Any(x => x.PublicId == storagePath))
                    return Result<PostDto>.Failure("Image not found.", HttpStatusCode.NotFound);

                var storage = storageFactory.Get(AttachmentType.Image);

                await storage.DeleteAsync(
                    storagePath,
                    AttachmentType.Image,
                    cancellationToken);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    post.RemoveImage(storagePath);

                    await postsRepository.UpdateAsync(post, ct);
                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                var dto = await postsReadModel.GetByIdAsync(
                    tenantId,
                    post.Id,
                    currentUserAccessor.GetUserId(),
                    cancellationToken);

                return dto is null
                    ? Result<PostDto>.Failure("Image removed but post could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<PostDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
