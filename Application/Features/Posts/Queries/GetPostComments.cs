using Application.Common;
using Application.Features.Posts.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Posts.Queries
{
    public sealed class GetPostComments
    {
        public sealed class Query : IRequest<Result<PagedList<PostCommentDto, DateTime?>>>
        {
            public required string PostId { get; set; }

            public int PageSize { get; set; } = 10;
            public DateTime? Cursor { get; set; }
        }

        public sealed class Handler(
            IPostsReadModel postsReadModel,
            ITenantContext tenantContext)
            : IRequestHandler<Query, Result<PagedList<PostCommentDto, DateTime?>>>
        {
            public async Task<Result<PagedList<PostCommentDto, DateTime?>>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PagedList<PostCommentDto, DateTime?>>.Failure(
                        "Tenant not found.",
                        HttpStatusCode.NotFound);

                var comments = await postsReadModel.GetCommentsAsync(
                    tenantId,
                    request.PostId,
                    request.PageSize,
                    request.Cursor,
                    cancellationToken);

                return Result<PagedList<PostCommentDto, DateTime?>>.Success(
                    comments,
                    HttpStatusCode.OK);
            }
        }
    }
}
