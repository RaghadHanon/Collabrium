using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Users.Commands;

public class DeleteUserSkills
{
    public class Command : IRequest<Result<Unit>>
    {
        public string? UserId { get; set; }
        public List<string> SkillIds { get; set; } = new();
    }

    public class Handler(
        IUsersRepository usersRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ICurrentUserAccessor currentUserAccessor
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var currentUser = await currentUserAccessor.GetCurrentUserAsync();
            if (currentUser == null)
                return Result<Unit>.Failure("Current user is not found.", HttpStatusCode.NotFound);

            var targetUserId = string.IsNullOrWhiteSpace(request.UserId)
                ? currentUser.Id
                : request.UserId;

            var user = await usersRepository.GetByIdAsync(targetUserId, cancellationToken);
            if (user is null)
                return Result<Unit>.Failure("User not found.", HttpStatusCode.NotFound);

            if (!string.IsNullOrWhiteSpace(tenantContext.TenantId) &&
                !string.Equals(user.TenantId, tenantContext.TenantId, StringComparison.Ordinal))
            {
                return Result<Unit>.Failure("User not found in this tenant.", HttpStatusCode.Forbidden);
            }

            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                await usersRepository.DeleteSkillsAsync(
                    targetUserId,
                    request.SkillIds,
                    cancellationToken);

            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}