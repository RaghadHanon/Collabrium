using Application.Common;
using Application.Interfaces;
using MediatR;
using System.Net;

namespace Application.Features.Identity.Commands;

public class ResetPassword
{
    public class Command : IRequest<Result<string>>
    {
        public required string UserId { get; set; }
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }

    public class Handler(IIdentityService identityService) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            await identityService.ResetPasswordAsync(request.UserId, request.Token, request.NewPassword);

            return Result<string>.Success(
                "Password has been reset successfully!",
                HttpStatusCode.OK
            );
        }
    }
}
