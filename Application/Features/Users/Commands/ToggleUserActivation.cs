using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Users.Commands;

public class ToggleUserActivation
{
    public class Command : IRequest<Result<Unit>>
    {
        public string UserId { get; set; } = default!;
    }

    public class Handler(
        IUsersRepository usersRepository,
        ITeamsRepository teamsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var currentUser =
                await currentUserAccessor.GetCurrentUserAsync();

            if (currentUser is null)
            {
                return Result<Unit>.Failure("Current user is not found.", HttpStatusCode.NotFound);
            }

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return Result<Unit>.Failure("UserId is required.", HttpStatusCode.BadRequest);
            }

            var userId = request.UserId.Trim();

            var user = await usersRepository.GetByIdAsync(userId, cancellationToken);

            if (user is null)
            {
                return Result<Unit>.Failure("User not found.", HttpStatusCode.NotFound);
            }

            var tenantId =
                tenantContext.CurrentTenant?.Id
                ?? tenantContext.TenantId
                ?? user.TenantId;

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);
            }

            tenantId = tenantId.Trim();

            if (!string.Equals(user.TenantId, tenantId, StringComparison.Ordinal))
            {
                return Result<Unit>.Failure("User not found in this tenant.", HttpStatusCode.Forbidden);
            }

            if (string.Equals(currentUser.Id,user.Id, StringComparison.Ordinal))
            {
                return Result<Unit>.Failure(
                    "You cannot change the activation status of your own account.",
                    HttpStatusCode.BadRequest);
            }

            var wasActive = user.IsActive;
            var willBeActive = !wasActive;

            if (!willBeActive)
            {
                var isLeadingAnyTeam =
                    await teamsRepository.IsUserLeadingAnyTeamAsync(
                        tenantId,
                        user.Id,
                        cancellationToken);

                if (isLeadingAnyTeam)
                {
                    return Result<Unit>.Failure(
                        "The user cannot be deactivated while they are assigned as a team lead. Reassign the teams to another team lead first.",
                        HttpStatusCode.BadRequest);
                }
            }

            await unitOfWork.ExecuteInTransactionAsync(
                async ct =>
                {
                    user.ToggleActivation();

                    await usersRepository.UpdateAsync(
                        user,
                        ct);
                },
                cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}