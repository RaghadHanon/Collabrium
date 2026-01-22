using Application.Features.Tenants.Commands;
using Application.Features.Tenants.DTOs;
using Application.Features.Tenants.Queries;
using AutoMapper;
using Domain.Entities;

public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        CreateMap<RegisterTenantWithAdmin.Command, Tenant>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.TenantName));

        CreateMap<Tenant, TenantDto>()
            .ForMember(d => d.ProfileImageURL,
                opt => opt.MapFrom(s => s.ProfileImage != null ? s.ProfileImage.Url : null));

        CreateMap<Tenant, TenantWithAdminDto>();
        CreateMap<TenantContactInfo, TenantContactInfoDto>();

        CreateMap<UpdateTenant.Command, Tenant>()
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<UpdateTenant.Command, TenantContactInfo>()
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<GetTenants.Query, TenantsSearchAndFilterParametersDto>();
    }
}
