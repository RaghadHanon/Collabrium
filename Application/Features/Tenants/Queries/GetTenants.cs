using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.Tenants.Queries;
public class GetTenants
{
    public class Query : IRequest<Result<PagedList<TenantDto, DateTime?>>>
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

    public class Handler(ITenantRepository tenantRepository, IMapper mapper)
        : IRequestHandler<Query, Result<PagedList<TenantDto, DateTime?>>>
    {
        public async Task<Result<PagedList<TenantDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var queryParams = mapper.Map<TenantsSearchAndFilterParametersDto>(request);

            var pagedUsers = await tenantRepository.GetTenantsAsync(queryParams, cancellationToken);
            var tenantDtos = pagedUsers.Items.Select(c => mapper.Map<TenantDto>(c)).ToList();

            return Result<PagedList<TenantDto, DateTime?>>.Success(
                new PagedList<TenantDto, DateTime?>
                {
                    Items = tenantDtos,
                    NextCursor = pagedUsers.NextCursor
                },
                HttpStatusCode.OK
            );
        }
    }
}