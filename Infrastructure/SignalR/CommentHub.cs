using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR;

public class CommentHub(IMediator mediator) : Hub
{
}
