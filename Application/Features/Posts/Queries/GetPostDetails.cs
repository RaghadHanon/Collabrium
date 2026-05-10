using Application.Common;
using Application.Features.Posts.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Posts.Queries
{
    public sealed class GetPostDetails
    {
        public sealed class Query : IRequest<Result<PostDetailsDto>>
        {
            public required string PostId { get; set; }
        }

        public sealed class Handler(
            IPostsReadModel postsReadModel,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor)
            : IRequestHandler<Query, Result<PostDetailsDto>>
        {
            public async Task<Result<PostDetailsDto>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PostDetailsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var dto = await postsReadModel.GetDetailsAsync(
                    tenantId,
                    request.PostId,
                    currentUserAccessor.GetUserId(),
                    cancellationToken);

                return dto is null
                    ? Result<PostDetailsDto>.Failure("Post not found.", HttpStatusCode.NotFound)
                    : Result<PostDetailsDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
