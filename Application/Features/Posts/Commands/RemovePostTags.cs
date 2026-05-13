using Application.Common;
using Application.Features.Posts.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Posts.Commands
{
    public sealed class RemovePostTags
    {
        public sealed class Command : IRequest<Result<PostDto>>
        {
            public required string PostId { get; set; }
            public List<string> TagIds { get; set; } = new();
        }

        public sealed class Handler(
            IPostsRepository postsRepository,
            IPostsReadModel postsReadModel,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<PostDto>>
        {
            public async Task<Result<PostDto>> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PostDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var post = await postsRepository.GetByIdAsync(
                    tenantId,
                    request.PostId,
                    cancellationToken);

                if (post is null)
                    if (post is null)
                        return Result<PostDto>.Failure("Post not found.", HttpStatusCode.NotFound);
                var currentUserId = currentUserAccessor.GetUserId();

                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<PostDto>.Failure(
                        "Current user not found.",
                        HttpStatusCode.Unauthorized);

                if (!string.Equals(post.CreatedByUserId, currentUserId, StringComparison.Ordinal))
                    return Result<PostDto>.Failure(
                        "You can modify tags only on your own post.",
                        HttpStatusCode.Forbidden);


                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    foreach (var tagId in request.TagIds)
                        post.RemoveTag(tagId);

                    await postsRepository.RemoveTagsAsync(
                        tenantId,
                        post.Id,
                        request.TagIds,
                        ct);

                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                var dto = await postsReadModel.GetByIdAsync(
                    tenantId,
                    post.Id,
                    null,
                    cancellationToken);

                return dto is null
                    ? Result<PostDto>.Failure("Post tags removed but post could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<PostDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
