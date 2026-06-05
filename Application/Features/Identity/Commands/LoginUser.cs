using Application.Common;
using Application.Events;
using Application.Features.Identity.DTOs;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Identity.Commands;

public class LoginUser
{
    public class Command : IRequest<Result<LoginResponseDto>>
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class Handler(
        IIdentityService identityService,
        IMediator mediator
    ) : IRequestHandler<Command, Result<LoginResponseDto>>
    {
        public async Task<Result<LoginResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var (
                accessToken,
                refreshToken,
                id,
                tenantId,
                mustChangePassword,
                fullName
            ) = await identityService.LoginAsync(
                request.Email,
                request.Password,
                cancellationToken);

            if (mustChangePassword && !string.IsNullOrWhiteSpace(tenantId))
            {
                await mediator.Publish(new UserFirstLoginWelcomeEvent(
                        tenantId,
                        id,
                        fullName
                    ), cancellationToken);
            }

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Id = id,
                TenantId = tenantId
            }, HttpStatusCode.OK);
        }
    }
}