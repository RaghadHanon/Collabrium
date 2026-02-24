using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Tenants.Queries;
public class GetTenantUsers
{
    public class Query : IRequest<Result<PagedList<UserDto, DateTime?>>>
    {
        public string? TenantId { get; set; }
        public string? NameContains { get; set; }
        public Department? Department { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? LastLoginAfter { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public UserRole? JobTitle { get; set; }
        public DateTime? Cursor { get; set; }
        public int PageSize { get; set; }
    }

    public class Handler(
        IUsersReadModel usersReadModel,
        IMapper mapper,
        ITenantContext tenantContext
        ) : IRequestHandler<Query, Result<PagedList<UserDto, DateTime?>>>
    {
        public async Task<Result<PagedList<UserDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var queryParams = mapper.Map<UsersSearchAndFilterParametersDto>(request);
            if (queryParams.TenantId is null)
                queryParams.TenantId = tenantContext.TenantId;

            var pagedUsers = await usersReadModel.GetUsersAsync(queryParams, cancellationToken);
            var usersDto = pagedUsers.Items.Select(c => mapper.Map<UserDto>(c)).ToList();

            return Result<PagedList<UserDto, DateTime?>>.Success(
                new PagedList<UserDto, DateTime?>
                {
                    Items = usersDto,
                    NextCursor = pagedUsers.NextCursor
                },
                HttpStatusCode.OK
            );
        }
    }
}
