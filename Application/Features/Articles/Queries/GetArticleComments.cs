using Application.Common;
using Application.Features.Articles.DTOs;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Articles.Queries
{
    public sealed class GetArticleComments
    {
        public sealed class Query : IRequest<Result<PagedList<ArticleCommentDto, DateTime?>>>
        {
            public required string ArticleId { get; set; }
            public int PageSize { get; set; } = 10;
            public DateTime? Cursor { get; set; }
        }

        public sealed class Handler(IArticleCommentsReadModel commentsReadModel)
            : IRequestHandler<Query, Result<PagedList<ArticleCommentDto, DateTime?>>>
        {
            public async Task<Result<PagedList<ArticleCommentDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var result = await commentsReadModel.GetCommentsAsync(
                    request.ArticleId,
                    request.PageSize,
                    request.Cursor,
                    cancellationToken);

                return Result<PagedList<ArticleCommentDto, DateTime?>>.Success(result, HttpStatusCode.OK);
            }
        }
    }
}
