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
        CreateMap<UpdateTenantRequest, UpdateTenant.Command>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name == "_NULL" ? "" : s.Name))
            .ForMember(d => d.Domain, opt => opt.MapFrom(s => s.Domain == "_NULL" ? "" : s.Domain))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description == "_NULL" ? "" : s.Description))
            .ForMember(d => d.Country, opt => opt.MapFrom(s => s.Country == "_NULL" ? "" : s.Country))
            .ForMember(d => d.WebsiteUrl, opt => opt.MapFrom(s => s.WebsiteURL == "_NULL" ? "" : s.WebsiteURL))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email == "_NULL" ? "" : s.Email))
            .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(s => s.PhoneNumber == "_NULL" ? "" : s.PhoneNumber))
            .ForMember(d => d.LinkedIn, opt => opt.MapFrom(s => s.LinkedIn == "_NULL" ? "" : s.LinkedIn))
            .ForMember(d => d.Instagram, opt => opt.MapFrom(s => s.Instagram == "_NULL" ? "" : s.Instagram))
            .ForMember(d => d.YouTube, opt => opt.MapFrom(s => s.YouTube == "_NULL" ? "" : s.YouTube))
            .ForMember(d => d.Facebook, opt => opt.MapFrom(s => s.Facebook == "_NULL" ? "" : s.Facebook));
    }
}