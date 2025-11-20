using Application.Features.Identity.DTOs;
using Application.Features.Tenants.DTOs;
using Application.Features.Tenants.Queries;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Identity;
public class IdentityMappingProfile : Profile
{
    public IdentityMappingProfile()
    {
        CreateMap<ApplicationUser, UserWithTenantDto>();
        CreateMap<ApplicationUser, UserDto>();
        CreateMap<ApplicationUser, TenantAdmin>();
        CreateMap<GetTenantUsers.Query, UsersSearchAndFilterParametersDto>();
    }
}
