using Application.Common;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Identity.Commands;

public class ReSendConfirmationEmail
{
    public class Command : IRequest<Result<string>>
    {
        public required string Email { get; set; }
    }

    public class Handler(IIdentityService identityService) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            await identityService.ResendConfirmationEmailAsync(request.Email, cancellationToken);

            return Result<string>.Success("Confirmation email resent successfully!", HttpStatusCode.OK);
        }
    }
}
