using Application.Common.DTOs;
using Application.Features.Tenants.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public sealed class TenantsReadProfile : Profile
{
    public TenantsReadProfile()
    {
        CreateMap<TenantEntity, TenantDto>()
            .ForMember(d => d.ProfileImageUrl,
                o => o.MapFrom(s => s.ProfileImage != null ? s.ProfileImage.Url : null));

        CreateMap<TenantEntity, TenantWithAdminDto>()
            .IncludeBase<TenantEntity, TenantDto>();

        CreateMap<ApplicationUserEntity, TenantAdminDto>();
        CreateMap<ContactDetailsOwned, ContactDetailsDto>();
        CreateMap<SocialLinksOwned, SocialLinksDto>();
    }
}