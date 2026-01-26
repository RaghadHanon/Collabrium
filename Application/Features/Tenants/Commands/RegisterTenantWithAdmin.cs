using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Tenants.Commands;

public class RegisterTenantWithAdmin
{
    public sealed class Command : IRequest<Result<UserWithTenantDto>>
    {
        public required string TenantName { get; init; }
        public required string Domain { get; init; }
        public required string Description { get; init; }

        public required string FullName { get; init; }
        public required string Email { get; init; }
        public required string Password { get; init; }
        public DateOnly DateOfBirth { get; init; }
        public required string PhoneNumber { get; init; }
    }

    public sealed class Handler : IRequestHandler<Command, Result<UserWithTenantDto>>
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IIdentityService _identityService;
        private readonly IUsersReadModel _usersReadModel;
        private readonly IUnitOfWork _uow;

        public Handler(
            ITenantRepository tenantRepository,
            IIdentityService identityService,
            IUsersReadModel usersReadModel,
            IUnitOfWork uow)
        {
            _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _usersReadModel = usersReadModel ?? throw new ArgumentNullException(nameof(usersReadModel));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<Result<UserWithTenantDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            // 1) business checks (write-model side)
            if (!await _tenantRepository.IsDomainUniqueAsync(request.Domain, cancellationToken))
                return Result<UserWithTenantDto>.Failure(
                    "A tenant with this domain already exists.",
                    HttpStatusCode.BadRequest);

            if (!await _tenantRepository.IsNameUniqueAsync(request.TenantName, cancellationToken))
                return Result<UserWithTenantDto>.Failure(
                    "A tenant with this name already exists.",
                    HttpStatusCode.BadRequest);

            // 2) create tenant aggregate
            var tenant = new Tenant(request.TenantName, request.Domain);
            tenant.UpdateProfile(request.Description, country: null, websiteUrl: null);

            await _tenantRepository.AddAsync(tenant, cancellationToken);

            // 3) create admin user through Identity
            var admin = await _identityService.RegisterUserAsync(
                fullName: request.FullName,
                department: Department.ITSupport,
                password: request.Password,
                dateOfBirth: request.DateOfBirth,
                role: UserRole.TenantAdmin,
                tenantId: tenant.Id,
                email: request.Email,
                phoneNumber: request.PhoneNumber);

            // 4) assign admin to tenant
            tenant.AssignAdmin(admin.Id);
            await _tenantRepository.UpdateAsync(tenant, cancellationToken);

            // 5) persist (UOW)
            await _uow.SaveChangesAsync(cancellationToken);

            // 6) read DTO (read-model side)
            var dto = await _usersReadModel.GetUserWithTenantAsync(admin.Id, cancellationToken);

            return dto is null
                ? Result<UserWithTenantDto>.Failure(
                    "User was created but could not be loaded.",
                    HttpStatusCode.InternalServerError)
                : Result<UserWithTenantDto>.Success(dto, HttpStatusCode.Created);
        }
    }
}