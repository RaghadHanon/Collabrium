using Application.Common.DTOs;
using Application.Features.Tenants.DTOs;
using Application.Features.Users.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public sealed class UsersReadProfile : Profile
{
    public UsersReadProfile()
    {
        CreateMap<ApplicationUserEntity, UserDto>()
            .ForMember(d => d.ProfileImageUrl, o => o.MapFrom(s => s.ProfileImage != null ? s.ProfileImage.Url : null));

        CreateMap<ApplicationUserEntity, TenantAdminDto>();
        CreateMap<ContactDetailsOwned, ContactDetailsDto>();
        CreateMap<SocialLinksOwned, SocialLinksDto>();
        CreateMap<ApplicationUserEntity, UserWithTenantDto>();
    }
}