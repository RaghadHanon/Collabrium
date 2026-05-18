using Application.Features.Announcements.DTOs;
using Application.Features.Users.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;
public sealed class AnnouncementsReadProfile : Profile
{
    public AnnouncementsReadProfile()
    {
        CreateMap<ApplicationUserEntity, UserDto>();

        CreateMap<AnnouncementAudienceEntity, AnnouncementAudienceDto>()
            .ForMember(
                d => d.TeamName,
                o => o.MapFrom(s =>
                    s.Team != null
                        ? s.Team.Name
                        : null));


        CreateMap<AnnouncementEntity, AnnouncementDto>()
            .ForMember(
                d => d.CreatedByUser,
                o => o.MapFrom(s => s.CreatedByUser))
            .ForMember(
                d => d.AudiencesCount,
                o => o.MapFrom(s => s.Audiences.Count))
            .ForMember(
                    d => d.Audiences,
                    o => o.MapFrom(s => s.Audiences))
            .ForMember(
                d => d.IsPublishedNow,
                o => o.MapFrom(s =>
                    s.PublishAt <= DateTime.UtcNow))
            .ForMember(
                d => d.IsExpired,
                o => o.MapFrom(s =>
                    s.ExpiresAt != null &&
                    s.ExpiresAt <= DateTime.UtcNow));

        CreateMap<AnnouncementEntity, AnnouncementDetailsDto>()
            .ForMember(
                d => d.CreatedByUser,
                o => o.MapFrom(s => s.CreatedByUser))
            .ForMember(
                d => d.Audiences,
                o => o.MapFrom(s => s.Audiences))
            .ForMember(
                d => d.IsPublishedNow,
                o => o.MapFrom(s =>
                    s.PublishAt <= DateTime.UtcNow))
            .ForMember(
                d => d.IsExpired,
                o => o.MapFrom(s =>
                    s.ExpiresAt != null &&
                    s.ExpiresAt <= DateTime.UtcNow));
    }
}