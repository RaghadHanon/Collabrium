using API.Contracts.Tenants;
using Application.Features.Tenants.Commands;
using AutoMapper;

namespace API.MappingProfiles;

public class TenantsMappingProfile : Profile
{
    public TenantsMappingProfile()
    {
        CreateMap<RegisterTenantWithAdminRequest, RegisterTenantWithAdmin.Command>();
    }
}
