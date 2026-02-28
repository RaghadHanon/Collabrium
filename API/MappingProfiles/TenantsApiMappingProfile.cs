using API.Contracts.Tenants;
using Application.Features.Tenants.Commands;
using Application.Features.Tenants.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public class TenantsApiMappingProfile : Profile
{
    public TenantsApiMappingProfile()
    {
        CreateMap<RegisterTenantWithAdminRequest, RegisterTenantWithAdmin.Command>();
        CreateMap<UploadImageRequest, UploadTenantProfilePhoto.Command>();
        CreateMap<GetTenantUsersRequest, GetTenantUsers.Query>();
        CreateMap<GetTenantsRequest, GetTenants.Query>();
        CreateMap<UpdateTenantRequest, UpdateTenant.Command>();
    }
}