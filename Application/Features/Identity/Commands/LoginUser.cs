using Application.Common;
using Application.Features.Identity.DTOs;
using Application.Interfaces;
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

    public class Handler(IIdentityService identityService)
        : IRequestHandler<Command, Result<LoginResponseDto>>
    {
        public async Task<Result<LoginResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var (token, id, tenantId) = await identityService.LoginAsync(request.Email, request.Password, cancellationToken);

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                Token = token,
                Id = id,
                TenantId = tenantId
            }, HttpStatusCode.OK);
        }
    }
}