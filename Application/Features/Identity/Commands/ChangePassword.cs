using Application.Common;
using Application.Interfaces;
using MediatR;
using System.Net;

namespace Application.Features.Identity.Commands;

public class ChangePassword
{
    public class Command : IRequest<Result<string>>
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }

    public class Handler(IIdentityService identityService, ICurrentUserAccessor currentUserAccessor
        ) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var currentUserId = currentUserAccessor.GetUserId();
            if (currentUserId == null)
                return Result<string>.Failure("You must login first", HttpStatusCode.Unauthorized);

            await identityService.ChangePasswordAsync(currentUserId, request.CurrentPassword, request.NewPassword);

            return Result<string>.Success(
                "Password has been changed successfully!",
                HttpStatusCode.OK
            );
        }
    }
}
