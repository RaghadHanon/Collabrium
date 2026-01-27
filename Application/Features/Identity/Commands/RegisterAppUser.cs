using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
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

    public class Handler(
        IIdentityService identityService,
        ITenantContext tenantContext,
        IUsersReadModel usersReadModel
        ) : IRequestHandler<Command, Result<UserWithTenantDto>>
    {
        public async Task<Result<UserWithTenantDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant =
                request.UserRole != UserRole.SuperAdmin
                    ? tenantContext.CurrentTenant!
                    : (await tenantContext.GetGlobalTenant(cancellationToken));

            if (tenant == null)
            {
                return Result<UserWithTenantDto>.Failure("Tenant not found."
                    , HttpStatusCode.BadRequest);
            }

            var user = await identityService.RegisterUserAsync(
                request.FullName,
                request.Department,
                request.Password,
                request.DateOfBirth,
                request.UserRole,
                tenant.Id,
                request.Email,
                request.PhoneNumber,
                cancellationToken);

            var dto = await usersReadModel.GetUserWithTenantAsync(user.Id, cancellationToken);

            return Result<UserWithTenantDto>.Success(dto, HttpStatusCode.Created);
        }
    }
}