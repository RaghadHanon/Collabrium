using Application.Common;
using Application.Features.Posts.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Posts.Queries
{
    public sealed class GetPostById
    {
        public sealed class Query : IRequest<Result<PostDto>>
        {
            public required string PostId { get; set; }
        }

        public sealed class Handler(
            IPostsReadModel postsReadModel,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor)
            : IRequestHandler<Query, Result<PostDto>>
        {
            public async Task<Result<PostDto>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PostDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var dto = await postsReadModel.GetByIdAsync(
                    tenantId,
                    request.PostId,
                    currentUserAccessor.GetUserId(),
                    cancellationToken);

                return dto is null
                    ? Result<PostDto>.Failure("Post not found.", HttpStatusCode.NotFound)
                    : Result<PostDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
