using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Tenants.Commands;

public class RegisterTenantWithAdmin
{
    public class Command : IRequest<Result<TenantWithAdminDto>>
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
        IIdentityService identityService,
        ITenantsReadModel tenantsReadModel,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<TenantWithAdminDto>>
    {
        public async Task<Result<TenantWithAdminDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!await tenantRepository.IsDomainUniqueAsync(request.Domain, cancellationToken))
                return Result<TenantWithAdminDto>.Failure(
                    "A tenant with this domain already exists.",
                    HttpStatusCode.BadRequest);

            if (!await tenantRepository.IsNameUniqueAsync(request.TenantName, cancellationToken))
                return Result<TenantWithAdminDto>.Failure(
                    "A tenant with this name already exists.",
                    HttpStatusCode.BadRequest);

            Tenant tenant = null!;
            string adminId = null!;

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                tenant = new Tenant(request.TenantName, request.Domain);
                tenant.UpdateProfile(request.TenantName, request.Domain, request.Description, country: null, websiteUrl: null);

                await tenantRepository.AddAsync(tenant, ct);

                var admin = await identityService.RegisterUserAsync(
                    fullName: request.FullName,
                    jobTitle: UserRole.TenantAdmin.ToString(),
                    department: Department.ITSupport,
                    password: request.Password,
                    dateOfBirth: request.DateOfBirth,
                    role: UserRole.TenantAdmin,
                    tenantId: tenant.Id,
                    email: request.Email,
                    phoneNumber: request.PhoneNumber,
                    cancellationToken: ct);

                adminId = admin.Id;

                tenant.AssignAdmin(adminId);
                await tenantRepository.UpdateAsync(tenant, ct);

            }, cancellationToken);

            var dto = await tenantsReadModel.GetWithAdminAsync(tenant.Id, cancellationToken);

            return dto is null
                ? Result<TenantWithAdminDto>.Failure(
                    "Tenant was created but could not be loaded.",
                    HttpStatusCode.InternalServerError)
                : Result<TenantWithAdminDto>.Success(dto, HttpStatusCode.Created);
        }
    }
}