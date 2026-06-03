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
        public sealed class Command : IRequest<Result<PostCommentDto>>
        {
            public required string PostId { get; set; }
            public required string Content { get; set; }
        }

        public sealed class Handler(
            IPostsRepository postsRepository,
            IPostCommentsRepository postCommentsRepository,
            IPostCommentsReadModel postCommentsReadModel,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<PostCommentDto>>
        {
            public async Task<Result<PostCommentDto>> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PostCommentDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();

                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<PostCommentDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

                var postExists = await postsRepository.ExistsAsync(
                    tenantId,
                    request.PostId,
                    cancellationToken);

                if (!postExists)
                    return Result<PostCommentDto>.Failure("Post not found.", HttpStatusCode.NotFound);

                PostComment? comment = null;

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    comment = new PostComment(
                        postId: request.PostId,
                        createdByUserId: currentUserId,
                        content: request.Content);

                    await postCommentsRepository.AddAsync(comment, ct);

                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                var dto = await postCommentsReadModel.GetByIdAsync(
                    tenantId,
                    request.PostId,
                    comment!.Id,
                    cancellationToken);

                return dto is null
                    ? Result<PostCommentDto>.Failure("Comment added but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<PostCommentDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
