using Application.Common;
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
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
                return Result<Unit>.Failure("User not found.", HttpStatusCode.NotFound);

            if (!string.IsNullOrWhiteSpace(tenantContext.TenantId) &&
                !string.Equals(user.TenantId, tenantContext.TenantId, StringComparison.Ordinal))
            {
                return Result<Unit>.Failure("User not found in this tenant.", HttpStatusCode.Forbidden);
            }

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await usersRepository.ReplaceUserRoleAsync(
                    request.UserId,
                    request.NewRole,
                    ct);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}