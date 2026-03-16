//using Domain.Events;
//using MediatR;

//namespace Application.EventsHandlers;

//public class WorkItemAssignedNotificationHandler
//    : INotificationHandler<WorkItemAssignedEvent>
//{
//    private readonly INotificationService notificationService;

//    public WorkItemAssignedNotificationHandler(INotificationService notificationService)
//    {
//        this.notificationService = notificationService;
//    }

//    public async Task Handle(WorkItemAssignedEvent notification, CancellationToken ct)
//    {
//        await notificationService.CreateAsync(new Notification
//        {
//            UserId = notification.AssigneeUserId,
//            Title = "New Work Item Assigned",
//            Message = $"{notification.WorkItemTitle} was assigned to you"
//        });
//    }
//}