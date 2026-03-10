using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.Tenants.Queries;
public class GetTenants
{
    public class Query : IRequest<Result<PagedList<TenantWithAdminDto, DateTime?>>>
    {
        public string? NameContains { get; set; }
        public string? DomainContains { get; set; }
        public string? DescriptionContains { get; set; }
        public string? Country { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Cursor { get; set; }
        public int PageSize { get; set; }
    }

    public class Handler(
        ITenantsReadModel tenantsReadModel,
        IMapper mapper
        ) : IRequestHandler<Query, Result<PagedList<TenantWithAdminDto, DateTime?>>>
    {
        public async Task<Result<PagedList<TenantWithAdminDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var queryParams = mapper.Map<TenantsSearchAndFilterParametersDto>(request);
            var pagedUsers = await tenantsReadModel.GetTenantsAsync(queryParams, cancellationToken);

            return Result<PagedList<TenantWithAdminDto, DateTime?>>.Success(pagedUsers, HttpStatusCode.OK);
        }
    }
}