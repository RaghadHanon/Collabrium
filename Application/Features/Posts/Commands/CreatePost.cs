using Application.Common;
using Application.Features.Posts.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using Domain.ValueObjects;
using MediatR;
using System.Net;

namespace Application.Features.Posts.Commands
{
    public sealed class CreatePost
    {
        public sealed class Command : IRequest<Result<PostDto>>
        {
            public required string Title { get; set; }
            public required string Content { get; set; }
            public required PostType Type { get; set; }

            public List<string> TagIds { get; set; } = new();
            public List<string> Links { get; set; } = new();
        }

        public sealed class Handler(
            IPostsRepository postsRepository,
            IPostsReadModel postsReadModel,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<PostDto>>
        {
            public async Task<Result<PostDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PostDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<PostDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

                Post? post = null;

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    post = new Post(
                        tenantId: tenantId,
                        createdByUserId: currentUserId,
                        title: request.Title,
                        content: request.Content,
                        type: request.Type);

                    post.SetTags(request.TagIds);

                    post.SetLinks(
                        request.Links
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Select(PostLink.Create));

                    await postsRepository.AddAsync(post, ct);

                    if (request.TagIds.Count > 0)
                    {
                        await postsRepository.SetTagsAsync(
                            tenantId,
                            post.Id,
                            request.TagIds,
                            ct);
                    }

                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                var dto = await postsReadModel.GetByIdAsync(
                    tenantId,
                    post!.Id,
                    currentUserId,
                    cancellationToken);

                return dto is null
                    ? Result<PostDto>.Failure("Post created but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<PostDto>.Success(dto, HttpStatusCode.Created);
            }
        }
    }
}
