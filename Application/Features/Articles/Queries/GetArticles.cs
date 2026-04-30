using Application.Common;
using Application.Features.Articles.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.Articles.Queries
{
    public sealed class GetArticles
    {
        public sealed class Query : IRequest<Result<PagedList<ArticleDto, DateTime?>>>
        {
            public string? Search { get; set; }
            public bool? IsPublished { get; set; }
            public string? CreatedByUserId { get; set; }

            public List<string>? TagIdsAny { get; set; }
            public List<string>? TagIdsAll { get; set; }

            public DateTime? CreatedAfter { get; set; }
            public DateTime? CreatedBefore { get; set; }

            public DateTime? UpdatedAfter { get; set; }
            public DateTime? UpdatedBefore { get; set; }

            public int PageSize { get; set; } = 10;
            public DateTime? Cursor { get; set; }
        }

        public sealed class Handler(
            IArticlesReadModel articlesReadModel,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IMapper mapper)
            : IRequestHandler<Query, Result<PagedList<ArticleDto, DateTime?>>>
        {
            public async Task<Result<PagedList<ArticleDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PagedList<ArticleDto, DateTime?>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var parameters = mapper.Map<ArticlesSearchAndFilterParametersDto>(request);
                parameters.TenantId = tenantId;

                var currentUserId = currentUserAccessor.GetUserId();

                var result = await articlesReadModel.GetArticlesAsync(
                    parameters,
                    currentUserId,
                    cancellationToken);

                return Result<PagedList<ArticleDto, DateTime?>>.Success(result, HttpStatusCode.OK);
            }
        }
    }
}

