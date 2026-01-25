using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Tenants.Commands;

public class RegisterTenantWithAdmin
{
    public class Command : IRequest<Result<UserWithTenantDto>>
    {
        public required string TenantName { get; set; }
        public required string Domain { get; set; }
        public required string Description { get; set; }

        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public required string PhoneNumber { get; set; }
    }

    public class Handler(
        ITenantRepository tenantRepository,
        IUsersRepository usersRepository,
        IIdentityService identityService,
        IMapper mapper
    ) : IRequestHandler<Command, Result<UserWithTenantDto>>
    {
        public async Task<Result<UserWithTenantDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!await tenantRepository.IsTenantDomainUniqueAsync(request.Domain, cancellationToken))
                return Result<UserWithTenantDto>.Failure("A tenant with this domain already exists.", HttpStatusCode.BadRequest);

            if (!await tenantRepository.IsTenantNameUniqueAsync(request.TenantName, cancellationToken))
                return Result<UserWithTenantDto>.Failure("A tenant with this name already exists.", HttpStatusCode.BadRequest);

            var tenant = mapper.Map<Tenant>(request);
            tenant.Activate();
            await tenantRepository.AddTenantAsync(tenant, cancellationToken);

            var admin = await identityService.RegisterUserAsync(
                request.FullName,
                Department.ITSupport,
                request.Password,
                request.DateOfBirth,
                UserRole.TenantAdmin,
                tenant.Id,
                request.Email,
                request.PhoneNumber);

            tenant.AssignAdmin(admin.Id);
            await tenantRepository.UpdateAsync(tenant, cancellationToken);
            await tenantRepository.SaveChangesAsync(cancellationToken);

            var dto = await usersRepository.GetUserWithTenantDtoAsync(admin.Id, cancellationToken);

            return Result<UserWithTenantDto>.Success(dto, HttpStatusCode.Created);
        }
    }
}