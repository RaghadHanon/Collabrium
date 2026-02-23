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
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
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
            var first = PatchHelpers.DecideStringPatch(request.FirstName);
            var last = PatchHelpers.DecideStringPatch(request.LastName);

            var currentFullName = user.FullName ?? "";
            var currentParts = currentFullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var currentFirst = currentParts.Length > 0 ? currentParts[0] : "";
            var currentLast = currentParts.Length > 1 ? string.Join(' ', currentParts.Skip(1)) : "";

            if (first.Action == PatchHelpers.PatchStringAction.Clear ||
                last.Action == PatchHelpers.PatchStringAction.Clear)
            {
                return Result<Unit>.Failure("FirstName/LastName cannot be cleared.", HttpStatusCode.BadRequest);
            }

            var newFirst = first.Action == PatchHelpers.PatchStringAction.Set ? first.Value! : currentFirst;
            var newLast = last.Action == PatchHelpers.PatchStringAction.Set ? last.Value! : currentLast;
            var newFullName = $"{newFirst} {newLast}".Trim();

            var newDob = request.DateOfBirth ?? user.DateOfBirth;

            user.UpdateIdentity(
                fullName: newFullName,
                email: user.Email,
                userName: user.UserName,
                dateOfBirth: newDob
            );

            user.SetContactInfo(
                email: user.Email,
                phone: PatchHelpers.ApplyStringPatch(user.ContactInfo.PhoneNumber, request.PhoneNumber),
                linkedIn: PatchHelpers.ApplyStringPatch(user.ContactInfo.Socials.LinkedIn, request.LinkedIn),
                facebook: PatchHelpers.ApplyStringPatch(user.ContactInfo.Socials.Facebook, request.Facebook),
                instagram: PatchHelpers.ApplyStringPatch(user.ContactInfo.Socials.Instagram, request.Instagram),
                youTube: PatchHelpers.ApplyStringPatch(user.ContactInfo.Socials.YouTube, request.YouTube),
                github: PatchHelpers.ApplyStringPatch(user.ContactInfo.Socials.Github, request.Github)
            );

            await usersRepository.UpdateAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}