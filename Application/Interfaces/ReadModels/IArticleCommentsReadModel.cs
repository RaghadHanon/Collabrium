using Application.Common;
using Application.Features.Articles.DTOs;

namespace Application.Interfaces.ReadModels
{
    public interface IArticleCommentsReadModel
    {
        Task<PagedList<ArticleCommentDto, DateTime?>> GetCommentsAsync(
            string articleId,
            int pageSize,
            DateTime? cursor,
            CancellationToken cancellationToken = default);

        Task<ArticleCommentDto?> GetByIdAsync(
            string articleId,
            string commentId,
            CancellationToken cancellationToken = default);
    }
}
