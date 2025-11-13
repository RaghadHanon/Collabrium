using Application.Features.Tenants.Commands;
using Application.Features.Tenants.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Tenants;
public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        CreateMap<RegisterTenantWithAdmin.Command, Tenant>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.TenantName));
        CreateMap<Tenant, TenantDto>();
    }
}
