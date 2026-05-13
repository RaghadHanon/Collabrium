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

            var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
                return Result<Unit>.Failure("User not found.", HttpStatusCode.NotFound);

            if (!string.IsNullOrWhiteSpace(tenantContext.TenantId) &&
                !string.Equals(user.TenantId, tenantContext.TenantId, StringComparison.Ordinal))
            {
                return Result<Unit>.Failure("User not found in this tenant.", HttpStatusCode.Forbidden);
            }

            var oldRole = user.JobTitle;
            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                await usersRepository.ReplaceUserRoleAsync(
                    request.UserId,
                    request.NewRole,
                    cancellationToken);
                user.ChangeJobTitle(request.NewRole);

                await usersRepository.UpdateAsync(user, cancellationToken);
            }, cancellationToken);

            await mediator.Publish(new UserRoleChangedEvent(
                       new UserSnapshotDto
                       {
                           Id = user.Id,
                           FullName = user.FullName,
                           Email = user.Email,
                           JobTitle = user.JobTitle
                       },
                        oldRole,
                        currentUser.FullName
                    ), cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}