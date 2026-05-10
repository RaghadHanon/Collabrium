using Application.Common;
using Application.Features.Posts.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.ValueObjects;
using MediatR;
using System.Net;

namespace Application.Features.Posts.Commands
{
    public sealed class UpdatePost
    {
        public sealed class Command : IRequest<Result<PostDto>>
        {
            public required string PostId { get; set; }

            public string? Title { get; set; }
            public string? Content { get; set; }
            public PostType? Type { get; set; }

            public List<string>? Links { get; set; }
        }

        public sealed class Handler(
            IPostsRepository postsRepository,
            IPostsReadModel postsReadModel,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<PostDto>>
        {
            public async Task<Result<PostDto>> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PostDto>.Failure(
                        "Tenant not found.",
                        HttpStatusCode.NotFound);

                var post = await postsRepository.GetByIdAsync(
                    tenantId,
                    request.PostId,
                    cancellationToken);

                if (post is null)
                    return Result<PostDto>.Failure(
                        "Post not found.",
                        HttpStatusCode.NotFound);

                var finalTitle =
                    PatchHelpers.ApplyStringPatch(post.Title, request.Title) ?? post.Title;

                var finalContent =
                    PatchHelpers.ApplyStringPatch(post.Content, request.Content) ?? post.Content;

                var finalType = request.Type ?? post.Type;

                if (finalType != PostType.Resource &&
                    request.Links is not null &&
                    request.Links.Any(x => !string.IsNullOrWhiteSpace(x)))
                {
                    return Result<PostDto>.Failure(
                        "Links are allowed only for resource posts.",
                        HttpStatusCode.BadRequest);
                }

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    post.UpdateDetails(
                        title: finalTitle,
                        content: finalContent,
                        type: finalType);

                    if (request.Links is not null)
                    {
                        post.SetLinks(
                            request.Links
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Select(PostLink.Create));
                    }

                    await postsRepository.UpdateAsync(post, ct);
                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                var dto = await postsReadModel.GetByIdAsync(
                    tenantId,
                    post.Id,
                    currentUserAccessor.GetUserId(),
                    cancellationToken);

                return dto is null
                    ? Result<PostDto>.Failure(
                        "Post updated but could not be loaded.",
                        HttpStatusCode.InternalServerError)
                    : Result<PostDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
