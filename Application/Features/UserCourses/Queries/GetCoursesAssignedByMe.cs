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
    public sealed class GetCoursesAssignedByMe
    {
        public sealed class Query : IRequest<Result<PagedList<AssignedUserCourseDto, DateTime?>>>
        {
            public string? Search { get; set; }

            public string? CourseId { get; set; }
            public string? AssignedToUserId { get; set; }

            public Domain.Enums.UserCourseStatus? Status { get; set; }
            public bool? IsSaved { get; set; }

            public DateTime? AssignedAfter { get; set; }
            public DateTime? AssignedBefore { get; set; }

            public DateTime? StartedAfter { get; set; }
            public DateTime? StartedBefore { get; set; }

            public DateTime? CompletedAfter { get; set; }
            public DateTime? CompletedBefore { get; set; }

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
            : IRequestHandler<Query, Result<PagedList<AssignedUserCourseDto, DateTime?>>>
        {
            public async Task<Result<PagedList<AssignedUserCourseDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PagedList<AssignedUserCourseDto, DateTime?>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<PagedList<AssignedUserCourseDto, DateTime?>>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

                var parameters = mapper.Map<AssignedUserCoursesSearchAndFilterParametersDto>(request);

                parameters.AssignedByUserId = currentUserId;

                var result = await userCoursesReadModel.GetAssignedByUserAsync(parameters, cancellationToken);

                return Result<PagedList<AssignedUserCourseDto, DateTime?>>.Success(result, HttpStatusCode.OK);
            }
        }
    }
}
