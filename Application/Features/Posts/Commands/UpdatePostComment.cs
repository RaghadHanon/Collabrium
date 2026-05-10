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
    public sealed class UpdatePostComment
    {
        public sealed class Command : IRequest<Result<PostCommentDto>>
        {
            public required string PostId { get; set; }
            public required string CommentId { get; set; }
            public required string Content { get; set; }
        }

        public sealed class Handler(
            IPostsRepository postsRepository,
            IPostsReadModel postsReadModel,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<PostCommentDto>>
        {
            public async Task<Result<PostCommentDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PostCommentDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<PostCommentDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

                var comment = await postsRepository.GetCommentByIdAsync(
                    request.PostId,
                    request.CommentId,
                    cancellationToken);

                if (comment is null)
                    return Result<PostCommentDto>.Failure("Comment not found.", HttpStatusCode.NotFound);

                if (comment.CreatedByUserId != currentUserId)
                    return Result<PostCommentDto>.Failure("You can update only your own comment.", HttpStatusCode.Forbidden);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    comment.Edit(request.Content);
                    await postsRepository.UpdateCommentAsync(comment, ct);
                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                var commentsPage = await postsReadModel.GetCommentsAsync(
                    tenantId,
                    request.PostId,
                    pageSize: 1,
                    cursor: null,
                    cancellationToken);

                var dto = commentsPage.Items.FirstOrDefault(x => x.Id == request.CommentId);

                return dto is null
                    ? Result<PostCommentDto>.Failure("Comment updated but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<PostCommentDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
