using Application.Features.Notifications.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public sealed class NotificationsReadProfile : Profile
{
    public NotificationsReadProfile()
    {
        CreateMap<NotificationEntity, NotificationDto>()
            .ForMember(
                d => d.TriggeredByUserName,
                o => o.MapFrom(s => s.TriggeredByUser != null ? s.TriggeredByUser.FullName : null));
    }
}