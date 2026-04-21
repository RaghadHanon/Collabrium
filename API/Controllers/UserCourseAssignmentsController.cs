using API.Contracts.UserCourses;
using API.SetupProgram;
using Application.Common;
using Application.Features.UserCourses.Commands;
using Application.Features.UserCourses.DTOs;
using Application.Features.UserCourses.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/user-course-assignments")]
    [ApiController]
    [Authorize(Policy = Policies.PasswordChanged)]
    public sealed class UserCourseAssignmentsController : ApiBaseController
    {
        /// <summary>
        /// Assigns a course to a specific user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/user-course-assignments/course/{courseId}/assign-user
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "assignedToUserId": "user-id-here"
        ///     }
        ///
        /// </remarks>
        /// <param name="courseId">The course id (from route).</param>
        /// <param name="request">Assignment payload.</param>
        /// <returns>The assigned user-course record.</returns>
        /// <response code="200">Course assigned successfully.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage courses.</response>
        /// <response code="404">If the tenant or course was not found.</response>
        [Authorize(Policy = Policies.Courses_Manage)]
        [HttpPost("course/{courseId}/assign-user")]
        public async Task<ActionResult<UserCourseDto>> AssignCourseToUser(
            string courseId,
            AssignCourseToUserRequest request)
        {
            var command = Mapper.Map<AssignCourseToUser.Command>(request);
            command.CourseId = courseId;

            return HandleResult(await Mediator.Send(command));
        }

        /// <summary>
        /// Assigns a course to all current members of a team.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/user-course-assignments/course/{courseId}/assign-team
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "teamId": "team-id-here"
        ///     }
        ///
        /// </remarks>
        /// <param name="courseId">The course id (from route).</param>
        /// <param name="request">Team assignment payload.</param>
        /// <returns>Assignment result message.</returns>
        /// <response code="200">Course assigned to team successfully.</response>
        /// <response code="400">If the request is invalid or the team has no members.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage courses.</response>
        /// <response code="404">If the tenant, course, or team was not found.</response>
        [Authorize(Policy = Policies.Courses_Manage)]
        [HttpPost("course/{courseId}/assign-team")]
        public async Task<ActionResult<string>> AssignCourseToTeam(
            string courseId,
            AssignCourseToTeamRequest request)
        {
            var command = Mapper.Map<AssignCourseToTeam.Command>(request);
            command.CourseId = courseId;

            return HandleResult(await Mediator.Send(command));
        }

        /// <summary>
        /// Removes assignment from a specific user for a given course.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/user-course-assignments/course/{courseId}/unassign-user
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "assignedToUserId": "user-id-here"
        ///     }
        ///
        /// </remarks>
        /// <param name="courseId">The course id (from route).</param>
        /// <param name="request">Unassignment payload.</param>
        /// <returns>Operation result message.</returns>
        /// <response code="200">Course unassigned successfully.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage courses.</response>
        /// <response code="404">If the tenant, course, or user-course was not found.</response>
        [Authorize(Policy = Policies.Courses_Manage)]
        [HttpPatch("course/{courseId}/unassign-user")]
        public async Task<ActionResult<string>> UnassignCourseFromUser(
            string courseId,
            UnassignCourseFromUserRequest request)
        {
            var command = Mapper.Map<UnassignCourseFromUser.Command>(request);
            command.CourseId = courseId;

            return HandleResult(await Mediator.Send(command));
        }

        /// <summary>
        /// Retrieves courses assigned by me with filtering and cursor pagination.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/user-course-assignments/assigned-by-me?search=sql&amp;status=Completed&amp;pageSize=10
        ///
        /// </remarks>
        /// <param name="request">Filtering and pagination parameters.</param>
        /// <returns>Paged list of assignments made by the current user.</returns>
        /// <response code="200">Assignments retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage courses.</response>
        /// <response code="404">If the tenant was not found.</response>
        [Authorize(Policy = Policies.Courses_Manage)]
        [HttpGet("assigned-by-me")]
        public async Task<ActionResult<PagedList<AssignedUserCourseDto, DateTime?>>> GetCoursesAssignedByMe(
            [FromQuery] GetCoursesAssignedByMeRequest request)
        {
            var query = Mapper.Map<GetCoursesAssignedByMe.Query>(request);
            return HandleResult(await Mediator.Send(query));
        }



    }
}
