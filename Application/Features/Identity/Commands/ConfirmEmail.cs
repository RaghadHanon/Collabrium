using Application.Common;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Identity.Commands;

public partial class ConfirmEmail
{
    public class Command : IRequest<Result<string>>
    {
        public required string UserId { get; set; }
        public required string Token { get; set; }
    }

    public class Handler(IIdentityService identityService) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            await identityService.ConfirmEmailAsync(request.UserId, request.Token, cancellationToken);
            return Result<string>.Success("Email confirmed successfully!", HttpStatusCode.OK);
        }
    }
}

