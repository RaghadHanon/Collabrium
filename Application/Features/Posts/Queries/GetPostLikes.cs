using Application.Common;
using Application.Features.Posts.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Posts.Queries;
public sealed class GetPostLikes
{
    public sealed class Query : IRequest<Result<PagedList<PostLikeUserDto, DateTime?>>>
    {
        public required string PostId { get; set; }
        public int PageSize { get; set; } = 10;
        public DateTime? Cursor { get; set; }
    }

    public sealed class Handler(
        IPostsRepository postsRepository,
        IPostsReadModel postsReadModel,
        ITenantContext tenantContext)
        : IRequestHandler<Query, Result<PagedList<PostLikeUserDto, DateTime?>>>
    {
        public async Task<Result<PagedList<PostLikeUserDto, DateTime?>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<PagedList<PostLikeUserDto, DateTime?>>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);

            var post = await postsRepository.GetByIdAsync(
                tenantId,
                request.PostId,
                cancellationToken);

            if (post is null)
                return Result<PagedList<PostLikeUserDto, DateTime?>>.Failure(
                    "Post not found.",
                    HttpStatusCode.NotFound);

            var result = await postsReadModel.GetPostLikesAsync(
                tenantId,
                request.PostId,
                request.PageSize,
                request.Cursor,
                cancellationToken);

            return Result<PagedList<PostLikeUserDto, DateTime?>>.Success(
                result,
                HttpStatusCode.OK);
        }
    }
}