using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Users.Commands;

public class UpdateUserContactDetails
{
    public class Command : IRequest<Result<Unit>>
    {
        public string? PhoneNumber { get; set; }
        public string? LinkedIn { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? YouTube { get; set; }
        public string? Github { get; set; }
    }

    public class Handler(
            IUsersRepository usersRepository,
            ICurrentUserAccessor currentUserAccessor,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Result<Unit>.Failure("User not found.", HttpStatusCode.NotFound);

            var user = await usersRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
                return Result<Unit>.Failure("User not found.", HttpStatusCode.NotFound);

            if (!string.IsNullOrWhiteSpace(tenantContext.TenantId) &&
                !string.Equals(user.TenantId, tenantContext.TenantId, StringComparison.Ordinal))
            {
                return Result<Unit>.Failure("User not found in this tenant.", HttpStatusCode.Forbidden);
            }

            user.SetContactInfo(
                email: user.Email,
                phone: request.PhoneNumber ?? user.ContactInfo.PhoneNumber,
                linkedIn: request.LinkedIn ?? user.ContactInfo.Socials.LinkedIn,
                facebook: request.Facebook ?? user.ContactInfo.Socials.Facebook,
                instagram: request.Instagram ?? user.ContactInfo.Socials.Instagram,
                youTube: request.YouTube ?? user.ContactInfo.Socials.YouTube,
                github: request.Github ?? user.ContactInfo.Socials.Github
            );

            await usersRepository.UpdateAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}