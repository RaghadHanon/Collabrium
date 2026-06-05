using MediatR;

namespace Application.Events;

public record UserFirstLoginWelcomeEvent(
    string TenantId,
    string UserId,
    string FullName
) : INotification;