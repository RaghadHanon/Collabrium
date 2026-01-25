using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.Users.Queries;

public class GetUser
{
    public class Query : IRequest<Result<UserWithTenantDto>>
    {
        public string? UserId { get; set; }
    }

    public class Handler(
        ICurrentUserAccessor currentUserAccessor,
        IUsersRepository usersRepository,
        ITenantContext tenantContext,
        IMapper mapper)
        : IRequestHandler<Query, Result<UserWithTenantDto>>
    {
        public async Task<Result<UserWithTenantDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userId = request.UserId ?? currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Result<UserWithTenantDto>.Failure("User not found", HttpStatusCode.NotFound);

            var user = await usersRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return Result<UserWithTenantDto>.Failure("User not found", HttpStatusCode.NotFound);

            var dto = mapper.Map<UserWithTenantDto>(user);

            var tenant = tenantContext.CurrentTenant;
            if (tenant is null)
                return Result<UserWithTenantDto>.Failure("Tenant not found", HttpStatusCode.NotFound);

            dto.Tenant = mapper.Map<TenantDto>(tenant);

            return Result<UserWithTenantDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}
