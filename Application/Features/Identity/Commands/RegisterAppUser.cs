using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Identity.Commands;

public class RegisterAppUser
{
    public class Command : IRequest<Result<UserWithTenantDto>>
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
        , IMapper mapper) : IRequestHandler<Command, Result<UserWithTenantDto>>
    {
        public async Task<Result<UserWithTenantDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await identityService.RegisterUserAsync(
                request.FullName,
                request.Department,
                request.Password,
                request.DateOfBirth,
                request.UserRole,
                request.UserRole != UserRole.SuperAdmin ? tenantContext.TenantId! :
                 (await tenantContext.GetGlobalTenant())!.Id,
                request.Email,
                request.PhoneNumber
            );

            user.MustChangePassword = request.UserRole != UserRole.SuperAdmin;
            var registeredSpecialistDto = mapper.Map<UserWithTenantDto>(user);

            return Result<UserWithTenantDto>.Success(registeredSpecialistDto, HttpStatusCode.Created);
        }
    }
}