using Application.Common;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Identity.Commands;

public class RevokeUserRefreshToken
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string RefreshToken { get; set; }
    }

    public class Handler(IIdentityService identityService)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            await identityService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}