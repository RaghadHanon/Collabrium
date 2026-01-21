using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.Tenants.Queries;
public class GetTenant
{
    public class Query : IRequest<Result<TenantWithAdminDto>>
    {
        public string? TenantId { get; set; }
    }

    public class Handler(IMapper mapper, ITenantRepository tenantRepository, ITenantContext tenantContext,
        IUsersRepository usersRepository)
        : IRequestHandler<Query, Result<TenantWithAdminDto>>
    {
        public async Task<Result<TenantWithAdminDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenant = request.TenantId == null ? tenantContext.CurrentTenant : await tenantRepository.GetTenantByIdAsync(request.TenantId!);
            if (tenant == null)
            {
                return Result<TenantWithAdminDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);
            }

            var tenantDto = mapper.Map<TenantWithAdminDto>(tenant);
            var adminUser = await usersRepository.GetByIdAsync(tenant.TenantAdminId!);
            if (adminUser != null)
            {
                tenantDto.TenantAdmin = mapper.Map<TenantAdmin>(adminUser);
            }

            return Result<TenantWithAdminDto>.Success(tenantDto, HttpStatusCode.OK);
        }
    }
}
