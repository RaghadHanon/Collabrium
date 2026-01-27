using Application.Features.Tenants.DTOs;
using Application.Features.Tenants.Queries;
using AutoMapper;

public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        CreateMap<GetTenants.Query, TenantsSearchAndFilterParametersDto>();
    }
}
