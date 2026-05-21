using Application.Features.Events.DTOs;
using Application.Features.Users.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;
public sealed class EventsReadProfile : Profile
{
    public EventsReadProfile()
    {
        string? currentUserId = null;

        CreateMap<ApplicationUserEntity, UserDto>()
            .ForMember(d => d.ProfileImageUrl,
                o => o.MapFrom(s =>
                    s.ProfileImage != null
                        ? s.ProfileImage.Url
                        : null));

        CreateMap<EventAttendanceEntity, EventAttendeeDto>()
            .ForMember(d => d.User,
                o => o.MapFrom(s => s.User));

        CreateMap<EventEntity, EventDto>()
            .ForMember(d => d.CreatedByUser,
                o => o.MapFrom(s => s.CreatedByUser))

            .ForMember(d => d.BannerImageUrl,
                o => o.MapFrom(s =>
                    s.BannerImage != null
                        ? s.BannerImage.Url
                        : null))

            .ForMember(d => d.BannerImagePublicId,
                o => o.MapFrom(s =>
                    s.BannerImage != null
                        ? s.BannerImage.PublicId
                        : null))

            .ForMember(d => d.AttendeesCount,
                o => o.MapFrom(s => s.Attendances.Count))

            .ForMember(d => d.AvailableSeats,
                o => o.MapFrom(s =>
                    s.MaxAttendees == null
                        ? (int?)null
                        : s.MaxAttendees.Value - s.Attendances.Count < 0
                            ? 0
                            : s.MaxAttendees.Value - s.Attendances.Count))

            .ForMember(d => d.IsCurrentUserInterested,
                o => o.MapFrom(s =>
                    currentUserId != null &&
                    s.Attendances.Any(a => a.UserId == currentUserId)))

            .ForMember(d => d.Status,
                o => o.Ignore());

        CreateMap<EventEntity, EventDetailsDto>()
            .IncludeBase<EventEntity, EventDto>()
            .ForMember(d => d.Attendees,
                o => o.MapFrom(s => s.Attendances));

        CreateMap<EventEntity, EventCalendarItemDto>()
            .ForMember(d => d.Status,
                o => o.Ignore());
    }
}