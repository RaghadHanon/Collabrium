using Application.Common;
using Application.Interfaces;
using MediatR;
using System.Net;

namespace Application.Features.Identity.Commands;

public class SendPasswordResetEmail
{
    public class Command : IRequest<Result<string>>
    {
        public required string Email { get; set; }
    }

    public class Handler(IIdentityService identityService) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            await identityService.SendPasswordResetEmailAsync(request.Email, cancellationToken);

            return Result<string>.Success(
                "Password reset email sent successfully!",
                HttpStatusCode.OK
            );
        }
    }
}
