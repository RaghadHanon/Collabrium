using Application.Common;
using Application.Features.Articles.DTOs;

namespace Application.Interfaces.ReadModels
{
    public interface IArticlesReadModel
    {
        Task<ArticleDto?> GetByIdAsync(
            string tenantId,
            string articleId,
            string? currentUserId = null,
            CancellationToken cancellationToken = default);

        Task<ArticleDetailsDto?> GetDetailsAsync(
            string tenantId,
            string articleId,
            string? currentUserId = null,
            CancellationToken cancellationToken = default);

        Task<PagedList<ArticleDto, DateTime?>> GetArticlesAsync(
            ArticlesSearchAndFilterParametersDto parameters,
            string? currentUserId = null,
            CancellationToken cancellationToken = default);
        Task<PagedList<ArticleLikeUserDto, DateTime?>> GetArticleLikesAsync(
            string tenantId,
            string articleId,
            int pageSize,
            DateTime? cursor,
            CancellationToken cancellationToken = default);
    }
}
