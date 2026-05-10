using Application.Common;
using Application.Features.Posts.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Posts.Commands
{
    public sealed class AddPostComment
    {
        public sealed class Command : IRequest<Result<PostDetailsDto>>
        {
            public required string PostId { get; set; }
            public required string Content { get; set; }
        }

        public sealed class Handler(
            IPostsRepository postsRepository,
            IPostsReadModel postsReadModel,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<PostDetailsDto>>
        {
            public async Task<Result<PostDetailsDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PostDetailsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<PostDetailsDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

                var postExists = await postsRepository.ExistsAsync(tenantId, request.PostId, cancellationToken);
                if (!postExists)
                    return Result<PostDetailsDto>.Failure("Post not found.", HttpStatusCode.NotFound);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    var comment = new PostComment(
                        postId: request.PostId,
                        createdByUserId: currentUserId,
                        content: request.Content);

                    await postsRepository.AddCommentAsync(comment, ct);
                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                var dto = await postsReadModel.GetDetailsAsync(
                    tenantId,
                    request.PostId,
                    currentUserId,
                    cancellationToken);

                return dto is null
                    ? Result<PostDetailsDto>.Failure("Comment added but post could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<PostDetailsDto>.Success(dto, HttpStatusCode.Created);
            }
        }
    }
}
