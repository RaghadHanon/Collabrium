using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Users.Commands;

public class UpdateUserProfile
{
    public class Command : IRequest<Result<Unit>>
    {
        public string? PhoneNumber { get; set; } // Identity field (see note below)
        public string? LinkedIn { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? YouTube { get; set; }
        public string? Github { get; set; }
    }

    public class Handler(
        IUsersRepository usersRepository,
        ICurrentUserAccessor currentUserAccessor,
        ITenantContext tenantContext)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant = tenantContext.CurrentTenant;
            if (tenant is null)
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var userId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Result<Unit>.Failure("User not found.", HttpStatusCode.NotFound);

            var user = await usersRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return Result<Unit>.Failure("User not found.", HttpStatusCode.NotFound);

            // Domain contact info update
            if (user.ContactInfo == null)
                user.SetContactInfo(new UserContactInfo());

            user.ContactInfo!.UpdateSocials(
                linkedIn: request.LinkedIn,
                facebook: request.Facebook,
                instagram: request.Instagram,
                youTube: request.YouTube);

            if (request.Github != null)
                user.ContactInfo.UpdateGithub(request.Github);

            // PhoneNumber is on Identity user entity. Two options:
            // 1) Add PhoneNumber to Domain.User and map it in UserMapper.
            // 2) Add method in repository to update phone number using UserManager.
            //
            // For now: ignore PhoneNumber here unless you choose option 1 or 2.

            await usersRepository.UpdateAsync(user, cancellationToken);
            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}
