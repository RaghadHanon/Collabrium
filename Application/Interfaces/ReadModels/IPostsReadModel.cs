using Application.Common;
using Application.Features.Posts.DTOs;

namespace Application.Interfaces.ReadModels
{
    public interface IPostsReadModel
    {
        Task<PagedList<PostDto, DateTime?>> GetPostsAsync(
            PostsSearchAndFilterParametersDto parameters,
            CancellationToken ct = default);

        Task<PostDto?> GetByIdAsync(
            string tenantId,
            string postId,
            string? currentUserId,
            CancellationToken ct = default);

        Task<PostDetailsDto?> GetDetailsAsync(
            string tenantId,
            string postId,
            string? currentUserId,
            CancellationToken ct = default);

        Task<PagedList<PostCommentDto, DateTime?>> GetCommentsAsync(
             string tenantId,
             string postId,
             int pageSize,
             DateTime? cursor,
             CancellationToken ct = default);
    }
}
