using Application.Common;
using Application.Features.Posts.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Features.Posts.Commands
{
    public sealed class UploadPostImages
    {
        public sealed class Command : IRequest<Result<PostDto>>
        {
            public required string PostId { get; set; }
            public List<IFormFile> Images { get; set; } = new();
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

                var currentUserId = currentUserAccessor.GetUserId();

                var post = await postsRepository.GetByIdAsync(tenantId, request.PostId, cancellationToken);
                if (post is null)
                    return Result<PostDto>.Failure("Post not found.", HttpStatusCode.NotFound);
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<PostDto>.Failure(
                        "Current user not found.",
                        HttpStatusCode.Unauthorized);

                if (!string.Equals(post.CreatedByUserId, currentUserId, StringComparison.Ordinal))
                    return Result<PostDto>.Failure(
                        "You can upload images only to your own post.",
                        HttpStatusCode.Forbidden);

                if (request.Images is null || request.Images.Count == 0)
                    return Result<PostDto>.Failure("No images uploaded.", HttpStatusCode.BadRequest);

                if (post.Images.Count + request.Images.Count > 5)
                    return Result<PostDto>.Failure("A post can have maximum 5 images.", HttpStatusCode.BadRequest);

                var storage = storageFactory.Get(AttachmentType.Image);
                var uploadedImages = new List<StoredImage>();

                foreach (var file in request.Images)
                {
                    if (file is null || file.Length == 0)
                        continue;

                    var stored = await storage.UploadAsync(
                        file,
                        AttachmentType.Image,
                        folder: "Collabrium/PostImages",
                        ct: cancellationToken);

                    if (stored is null)
                        return Result<PostDto>.Failure("Failed to upload one or more images.", HttpStatusCode.BadRequest);

                    uploadedImages.Add(StoredImage.Create(stored.Url, stored.StoragePath));
                }

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    foreach (var image in uploadedImages)
                        post.AddImage(image);

                    await postsRepository.UpdateAsync(post, ct);
                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                var dto = await postsReadModel.GetByIdAsync(
                    tenantId,
                    post.Id,
                    currentUserId,
                    cancellationToken);

                return dto is null
                    ? Result<PostDto>.Failure("Images uploaded but post could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<PostDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
