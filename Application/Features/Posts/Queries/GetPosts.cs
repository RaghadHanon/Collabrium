using Application.Common;
using Application.Features.Posts.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.Posts.Queries
{
    public sealed class GetPosts
    {
        public sealed class Query : IRequest<Result<PagedList<PostDto, DateTime?>>>
        {
            public string? Search { get; set; }
            public string? CreatedByUserId { get; set; }

            public Domain.Enums.PostType? Type { get; set; }
            public bool? IsPinned { get; set; }

            public List<string>? TagIdsAny { get; set; }
            public List<string>? TagIdsAll { get; set; }

            public bool? HasImages { get; set; }
            public bool? HasLinks { get; set; }

            public DateTime? CreatedAfter { get; set; }
            public DateTime? CreatedBefore { get; set; }

            public DateTime? UpdatedAfter { get; set; }
            public DateTime? UpdatedBefore { get; set; }

            public int PageSize { get; set; } = 10;
            public DateTime? Cursor { get; set; }
        }

        public sealed class Handler(
            IPostsReadModel postsReadModel,
            ITenantContext tenantContext,
            IMapper mapper,
            ICurrentUserAccessor currentUserAccessor)
            : IRequestHandler<Query, Result<PagedList<PostDto, DateTime?>>>
        {
            public async Task<Result<PagedList<PostDto, DateTime?>>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PagedList<PostDto, DateTime?>>.Failure(
                        "Tenant not found.",
                        HttpStatusCode.NotFound);

                var parameters = mapper.Map<PostsSearchAndFilterParametersDto>(request);

                parameters.TenantId = tenantId;
                parameters.CurrentUserId = currentUserAccessor.GetUserId();

                var result = await postsReadModel.GetPostsAsync(parameters, cancellationToken);

                return Result<PagedList<PostDto, DateTime?>>.Success(result, HttpStatusCode.OK);
            }
        }
    }
}
