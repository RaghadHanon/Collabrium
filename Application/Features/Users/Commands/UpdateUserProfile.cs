using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System.Net;

namespace Application.Features.Users.Commands;
public class UpdateUserProfile
{
    public class Command : IRequest<Result<Unit>>
    {
        // Contact Info
        public string? PhoneNumber { get; set; }
        public string? LinkedIn { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? YouTube { get; set; }
        public string? Github { get; set; }
    }

    public class Handler(
        IMapper mapper,
        IUsersRepository usersRepository,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant = tenantContext.CurrentTenant;
            if (tenant == null)
            {
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);
            }

            var user = await currentUserAccessor.GetUserAsync();
            if (user == null)
            {
                return Result<Unit>.Failure("User not found.", HttpStatusCode.NotFound);
            }

            user.UserContactInfo ??= new UserContactInfo
            {
                UserId = user.Id
            };

            mapper.Map(request, user.UserContactInfo);

            await usersRepository.SaveChangesAsync();

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}
