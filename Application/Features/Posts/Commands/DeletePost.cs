using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
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
