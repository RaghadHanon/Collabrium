using Application.Common;
using Application.Features.Identity.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Identity.Commands;

public class RegisterAppUser
{
    public class Command : IRequest<Result<UserDto>>
    {
        public required string FullName { get; set; }
        public Department Department { get; set; }
        public UserRole UserRole { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public required string PhoneNumber { get; set; }
    }

    public class Handler(IIdentityService identityService, ITenantContext tenantContext
        , IMapper mapper) : IRequestHandler<Command, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await identityService.RegisterUserAsync(
                request.FullName,
                request.Department,
                request.Password,
                request.DateOfBirth,
                request.UserRole,
                tenantContext.TenantId!,
                request.Email,
                request.PhoneNumber
            );

            user.MustChangePassword = (request.UserRole != UserRole.SuperAdmin);
            var registeredSpecialistDto = mapper.Map<UserDto>(user);

            return Result<UserDto>.Success(registeredSpecialistDto, HttpStatusCode.Created);
        }
    }
}