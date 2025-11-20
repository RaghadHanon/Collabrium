using Application.Common;
using Application.Features.Identity.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Features.Tenants.Commands;

public class RegisterTenantWithAdmin
{
    public class Command : IRequest<Result<UserWithTenantDto>>
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
    ) : IRequestHandler<Command, Result<UserWithTenantDto>>
    {
        public async Task<Result<UserWithTenantDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var isTenantDomainUnique = await tenantRepository.IsTenantDomainUniqueAsync(request.Domain, cancellationToken);
            if (!isTenantDomainUnique)
            {
                return Result<UserWithTenantDto>.Failure("A tenant with this domain already exists.", HttpStatusCode.BadRequest);
            }

            var isTenantNameUnique = await tenantRepository.IsTenantNameUniqueAsync(request.TenantName, cancellationToken);
            if (!isTenantNameUnique)
            {
                return Result<UserWithTenantDto>.Failure("A tenant with this name already exists.", HttpStatusCode.BadRequest);
            }

            var tenant = mapper.Map<Tenant>(request);
            tenant.IsActive = true;

            await tenantRepository.AddTenantAsync(tenant);

            var user = await identityService.RegisterUserAsync(
                request.FullName,
                Department.ITSupport, 
                request.Password,
                request.DateOfBirth,
                UserRole.TenantAdmin,
                tenant.Id,
                request.Email,
                request.PhoneNumber
            );

            tenant.TenantAdminId = user.Id;
            user.MustChangePassword = true;
            tenant.Users.Add(user);
            await tenantRepository.SaveChangesAsync(cancellationToken);

            var dto = mapper.Map<UserWithTenantDto>(user);

            return Result<UserWithTenantDto>.Success(dto, HttpStatusCode.Created);
        }
    }
}
