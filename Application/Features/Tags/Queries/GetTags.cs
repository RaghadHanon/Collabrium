using Application.Common;
using Application.Features.Tags.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.Tags.Queries
{
    public sealed class GetTags
    {
        public sealed class Query : IRequest<Result<PagedList<TagDto, DateTime?>>>
        {
            public string? TenantId { get; set; }
            public string? NameContains { get; set; }

            public DateTime? CreatedAfter { get; set; }
            public DateTime? CreatedBefore { get; set; }

            public DateTime? Cursor { get; set; }
            public int PageSize { get; set; }
        }

        public sealed class Handler(
            ITagsReadModel tagsReadModel,
            IMapper mapper
        ) : IRequestHandler<Query, Result<PagedList<TagDto, DateTime?>>>
        {
            public async Task<Result<PagedList<TagDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var parameters = mapper.Map<TagsSearchParametersDto>(request);

                var list = await tagsReadModel.GetAsync(parameters, cancellationToken);

                return Result<PagedList<TagDto, DateTime?>>.Success(list, HttpStatusCode.OK);
            }
        }
    }
}

