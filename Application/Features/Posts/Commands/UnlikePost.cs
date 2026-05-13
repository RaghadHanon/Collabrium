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
    public sealed class UnlikePost
    {
        public sealed class Command : IRequest<Result<PostDto>>
        {
            public required string PostId { get; set; }
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
                    return Result<PostDto>.Failure(
                        "Tenant not found.",
                        HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();

                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<PostDto>.Failure(
                        "Current user not found.",
                        HttpStatusCode.Unauthorized);

                var postExists = await postsRepository.ExistsAsync(
                    tenantId,
                    request.PostId,
                    cancellationToken);

                if (!postExists)
                    return Result<PostDto>.Failure(
                        "Post not found.",
                        HttpStatusCode.NotFound);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    await postsRepository.RemoveLikeAsync(
                        request.PostId,
                        currentUserId,
                        ct);

                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                var dto = await postsReadModel.GetByIdAsync(
                    tenantId,
                    request.PostId,
                    currentUserId,
                    cancellationToken);

                return dto is null
                    ? Result<PostDto>.Failure(
                        "Post unliked but could not be loaded.",
                        HttpStatusCode.InternalServerError)
                    : Result<PostDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
