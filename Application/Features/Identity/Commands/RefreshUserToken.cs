using Application.Common;
using Application.Features.Identity.DTOs;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Identity.Commands;

public class RefreshUserToken
{
    public class Command : IRequest<Result<RefreshTokenResponseDto>>
    {
        public required string RefreshToken { get; set; }
    }

    public class Handler(IIdentityService identityService)
        : IRequestHandler<Command, Result<RefreshTokenResponseDto>>
    {
        public async Task<Result<RefreshTokenResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var (accessToken, refreshToken) =
                await identityService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

            return Result<RefreshTokenResponseDto>.Success(new RefreshTokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }, HttpStatusCode.OK);
        }
    }
}