using Application.Common;
using Application.Events;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Users.Commands;

public class UpdateUserRole
{
    public class Command : IRequest<Result<Unit>>
    {
        public string UserId { get; set; } = default!;
        public string NewRole { get; set; } = default!;
    }

    public class Handler(
        IUsersRepository usersRepository,
        ITeamsRepository teamsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        IMediator mediator,
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
                return Result<Unit>.Failure(
                    "Current user is not found.",
                    HttpStatusCode.NotFound);
            }

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return Result<Unit>.Failure(
                    "UserId is required.",
                    HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(request.NewRole))
            {
                return Result<Unit>.Failure(
                    "NewRole is required.",
                    HttpStatusCode.BadRequest);
            }

            var userId = request.UserId.Trim();
            var newRole = request.NewRole.Trim();

            var user = await usersRepository.GetByIdAsync(
                userId,
                cancellationToken);

            if (user is null)
            {
                return Result<Unit>.Failure(
                    "User not found.",
                    HttpStatusCode.NotFound);
            }

            var tenantId =
                tenantContext.CurrentTenant?.Id
                ?? tenantContext.TenantId
                ?? user.TenantId;

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Result<Unit>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);
            }

            tenantId = tenantId.Trim();

            if (!string.Equals(
                    user.TenantId,
                    tenantId,
                    StringComparison.Ordinal))
            {
                return Result<Unit>.Failure(
                    "User not found in this tenant.",
                    HttpStatusCode.Forbidden);
            }

            var oldRole = user.JobTitle;

            var isRoleChanging = !string.Equals(
                oldRole?.Trim(),
                newRole,
                StringComparison.OrdinalIgnoreCase);

            if (isRoleChanging)
            {
                var isLeadingAnyTeam =
                    await teamsRepository.IsUserLeadingAnyTeamAsync(
                        tenantId,
                        user.Id,
                        cancellationToken);

                if (isLeadingAnyTeam)
                {
                    return Result<Unit>.Failure(
                        "The user's role cannot be changed while they are assigned as a team lead. Reassign the teams to another team lead first.",
                        HttpStatusCode.Conflict);
                }
            }

            await unitOfWork.ExecuteInTransactionAsync(
                async ct =>
                {
                    await usersRepository.ReplaceUserRoleAsync(
                        user.Id,
                        newRole,
                        ct);

                    user.ChangeJobTitle(newRole);

                    await usersRepository.UpdateAsync(
                        user,
                        ct);
                },
                cancellationToken);

            await mediator.Publish(
                new UserRoleChangedEvent(
                    tenantId,
                    new UserSnapshotDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        JobTitle = user.JobTitle
                    },
                    oldRole,
                    currentUser.Id,
                    currentUser.FullName),
                cancellationToken);

            return Result<Unit>.Success(
                Unit.Value,
                HttpStatusCode.NoContent);
        }
    }
}