using Application.Common;
using Application.Features.Tags.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.Tags.Queries;

public sealed class GetTags
{
    public sealed class Query : IRequest<Result<PagedList<TagDetailsDto, DateTime?>>>
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
    ) : IRequestHandler<Query, Result<PagedList<TagDetailsDto, DateTime?>>>
    {
        public async Task<Result<PagedList<TagDetailsDto, DateTime?>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var parameters = mapper.Map<TagsSearchAndFilterParametersDto>(request);

            var list = await tagsReadModel.GetAsync(parameters, cancellationToken);

            return Result<PagedList<TagDetailsDto, DateTime?>>.Success(
                list,
                HttpStatusCode.OK);
        }
    }
}

