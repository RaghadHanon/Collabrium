using Application.Common;
using Application.Features.Tags.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Tags.Queries
{
    public sealed class GetTag
    {
        public sealed class Query : IRequest<Result<TagDetailsDto>>
        {
            public required string TagId { get; set; }
        }

        public sealed class Handler(
            ITagsReadModel tagsReadModel,
            ITenantContext tenantContext
        ) : IRequestHandler<Query, Result<TagDetailsDto>>
        {
            public async Task<Result<TagDetailsDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<TagDetailsDto>.Failure(
                        "Tenant not found.",
                        HttpStatusCode.NotFound);

                var dto = await tagsReadModel.GetByIdAsync(
                    tenantId,
                    request.TagId,
                    cancellationToken);

                return dto is null
                    ? Result<TagDetailsDto>.Failure(
                        "Tag not found.",
                        HttpStatusCode.NotFound)
                    : Result<TagDetailsDto>.Success(
                        dto,
                        HttpStatusCode.OK);
            }
        }
    }
}
