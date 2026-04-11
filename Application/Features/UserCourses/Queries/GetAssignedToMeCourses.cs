using Application.Common;
using Application.Features.UserCourses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserCourses.Queries
{
    public sealed class GetAssignedToMeCourses
    {
        public sealed class Query : IRequest<Result<PagedList<UserCourseDto, DateTime?>>>
        {
            public string? Search { get; set; }

            public DateTime? AssignedAfter { get; set; }
            public DateTime? AssignedBefore { get; set; }

            public DateTime? CreatedAfter { get; set; }
            public DateTime? CreatedBefore { get; set; }

            public DateTime? UpdatedAfter { get; set; }
            public DateTime? UpdatedBefore { get; set; }

            public int PageSize { get; set; } = 10;
            public DateTime? Cursor { get; set; }
        }

        public sealed class Handler(
            IUserCoursesReadModel userCoursesReadModel,
            ITenantContext tenantContext,
            IMapper mapper,
            ICurrentUserAccessor currentUserAccessor)
            : IRequestHandler<Query, Result<PagedList<UserCourseDto, DateTime?>>>
        {
            public async Task<Result<PagedList<UserCourseDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PagedList<UserCourseDto, DateTime?>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<PagedList<UserCourseDto, DateTime?>>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

                var parameters = mapper.Map<UserCoursesSearchAndFilterParametersDto>(request);
                parameters.UserId = currentUserId;
                parameters.IsAssigned = true;

                var result = await userCoursesReadModel.GetUserCoursesAsync(parameters, cancellationToken);

                return Result<PagedList<UserCourseDto, DateTime?>>.Success(result, HttpStatusCode.OK);
            }
        }
    }
}
