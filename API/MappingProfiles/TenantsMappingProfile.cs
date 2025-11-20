using API.Contracts.Tenants;
using Application.Features.Tenants.Commands;
using Application.Features.Tenants.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public class TenantsMappingProfile : Profile
{
    public TenantsMappingProfile()
    {
        CreateMap<RegisterTenantWithAdminRequest, RegisterTenantWithAdmin.Command>();
        CreateMap<UploadProfileImageRequest, UploadTenantProfileImage.Command>();
        CreateMap<GetTenantUsersRequest, GetTenantUsers.Query>();
    }
}
