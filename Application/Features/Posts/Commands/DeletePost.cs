using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Posts.Commands
{
    public sealed class DeletePost
    {
        public sealed class Command : IRequest<Result<Unit>>
        {
            public required string PostId { get; set; }
        }

        public sealed class Handler(
            IPostsRepository postsRepository,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<Unit>.Failure(
                        "Tenant not found.",
                        HttpStatusCode.NotFound);

                var post = await postsRepository.GetByIdAsync(
                    tenantId,
                    request.PostId,
                    cancellationToken);

                if (post is null)
                    return Result<Unit>.Failure(
                        "Post not found.",
                        HttpStatusCode.NotFound);
                var currentUserId = currentUserAccessor.GetUserId();

                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<Unit>.Failure(
                        "Current user not found.",
                        HttpStatusCode.Unauthorized);

                var isOwner = string.Equals(
                    post.CreatedByUserId,
                    currentUserId,
                    StringComparison.Ordinal);

                var role = await currentUserAccessor.GetUserRole(
                 currentUserId,
                 cancellationToken);

                var isManager =
                    role is UserRole.TenantAdmin
                        or UserRole.CEO
                        or UserRole.HR
                        or UserRole.IT;
                if (!isOwner && !isManager)
                {
                    return Result<Unit>.Failure(
                        "You can delete only your own post unless you are a manager.",
                        HttpStatusCode.Forbidden);
                }

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    await postsRepository.DeleteAsync(
                        tenantId,
                        request.PostId,
                        ct);

                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                return Result<Unit>.Success(Unit.Value, HttpStatusCode.OK);
            }
        }
    }
}
