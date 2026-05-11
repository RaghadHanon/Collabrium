using Application.Common;
using Application.Features.Posts.DTOs;

namespace Application.Interfaces.ReadModels
{
    public interface IPostCommentsReadModel
    {
        Task<PagedList<PostCommentDto, DateTime?>> GetCommentsAsync(
            string tenantId,
            string postId,
            int pageSize,
            DateTime? cursor,
            CancellationToken ct = default);

        Task<PostCommentDto?> GetByIdAsync(
            string tenantId,
            string postId,
            string commentId,
            CancellationToken ct = default);
    }
}
