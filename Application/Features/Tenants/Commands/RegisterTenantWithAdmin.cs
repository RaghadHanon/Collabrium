using Application.Common;
using Application.Features.Identity.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Tenants.Commands;

public class RegisterTenantWithAdmin
{
    public class Command : IRequest<Result<UserDto>>
    {
        public required string TenantName { get; set; }
        public required string Domain { get; set; }
        public required string Description { get; set; }

        // OrganizationIT details
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public required string PhoneNumber { get; set; }
    }

    public class Handler(
        ITenantRepository tenantRepository,
        IIdentityService identityService,
        IMapper mapper
    ) : IRequestHandler<Command, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var isTenantDomainUnique = await tenantRepository.IsTenantDomainUniqueAsync(request.Domain, cancellationToken);
            if (!isTenantDomainUnique)
            {
                return Result<UserDto>.Failure("A tenant with this domain already exists.", HttpStatusCode.BadRequest);
            }

            var isTenantNameUnique = await tenantRepository.IsTenantNameUniqueAsync(request.TenantName, cancellationToken);
            if (!isTenantNameUnique)
            {
                return Result<UserDto>.Failure("A tenant with this name already exists.", HttpStatusCode.BadRequest);
            }

            var tenant = mapper.Map<Tenant>(request);
            tenant.IsActive = true;

            await tenantRepository.AddTenantAsync(tenant);

            var user = await identityService.RegisterUserAsync(
                request.FullName,
                Department.ITSupport, 
                request.Password,
                request.DateOfBirth,
                UserRole.OrganizationIT,
                tenant.Id,
                request.Email,
                request.PhoneNumber
            );

            user.MustChangePassword = true;
            tenant.Users.Add(user);
            await tenantRepository.SaveChangesAsync(cancellationToken);

            var dto = mapper.Map<UserDto>(user);

            return Result<UserDto>.Success(dto, HttpStatusCode.Created);
        }
    }
}
