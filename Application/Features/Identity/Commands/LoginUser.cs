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
        public async Task<Result<LoginResponseDto>> Handle(Command request
            , CancellationToken cancellationToken)
        {
            var (token, id) = await identityService
                .AuthenticateAsync(request.Email, request.Password);

            var user = await identityService.GetUserAsync(request.Email);

            if (!await identityService.IsEmailConfirmedAsync(user!, cancellationToken))
                return Result<LoginResponseDto>.Failure("You must confirm your email before logging in.", HttpStatusCode.Unauthorized);

            user!.LastLoginAt = DateTime.UtcNow;
            await identityService.UpdateUserAsync(user);

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                Token = token,
                Id = id
            }, HttpStatusCode.OK);
        }
    }
}