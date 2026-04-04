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
    [Route("api/user-courses")]
    [ApiController]
    [Authorize(Policy = Policies.PasswordChanged)]
    public sealed class UserCoursesController : ApiBaseController
    {
        /// <summary>
        /// Retrieves my user courses with filtering and cursor pagination.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/user-courses?search=sql&amp;status=InProgress&amp;isSaved=true&amp;pageSize=10
        ///
        /// </remarks>
        /// <param name="request">Filtering and pagination parameters.</param>
        /// <returns>Paged list of my user courses.</returns>
        /// <response code="200">User courses retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the tenant was not found.</response>
        [HttpGet]
        public async Task<ActionResult<PagedList<UserCourseDto, DateTime?>>> GetMyUserCourses(
            [FromQuery] GetMyUserCoursesRequest request)
        {
            var query = Mapper.Map<GetMyUserCourses.Query>(request);
            return HandleResult(await Mediator.Send(query));
        }

        /// <summary>
        /// Retrieves my saved user courses only.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/user-courses/saved?search=backend&amp;pageSize=10
        ///
        /// </remarks>
        /// <param name="request">Filtering and pagination parameters.</param>
        /// <returns>Paged list of my saved courses.</returns>
        /// <response code="200">Saved user courses retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the tenant was not found.</response>
        [HttpGet("saved")]
        public async Task<ActionResult<PagedList<UserCourseDto, DateTime?>>> GetMySavedUserCourses(
            [FromQuery] GetMySavedUserCoursesRequest request)
        {
            var query = Mapper.Map<GetMySavedUserCourses.Query>(request);
            return HandleResult(await Mediator.Send(query));
        }

        /// <summary>
        /// Retrieves my user course by course id.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/user-courses/course/{courseId}
        ///
        /// </remarks>
        /// <param name="courseId">The course id.</param>
        /// <returns>The user course record for this course.</returns>
        /// <response code="200">User course retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the tenant or user course was not found.</response>
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<UserCourseDto>> GetMyUserCourseByCourseId(string courseId)
        {
            var query = new GetMyUserCourseByCourseId.Query
            {
                CourseId = courseId
            };

            return HandleResult(await Mediator.Send(query));
        }

        /// <summary>
        /// Retrieves user course details by user course id.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/user-courses/{userCourseId}
        ///
        /// </remarks>
        /// <param name="userCourseId">The user course id.</param>
        /// <returns>User course details.</returns>
        /// <response code="200">User course details retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the tenant or user course was not found.</response>
        [HttpGet("{userCourseId}")]
        public async Task<ActionResult<UserCourseDetailsDto>> GetUserCourseById(string userCourseId)
        {
            var query = new GetUserCourseById.Query
            {
                UserCourseId = userCourseId
            };

            return HandleResult(await Mediator.Send(query));
        }

        /// <summary>
        /// Saves a course to my saved list.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/user-courses/course/{courseId}/save
        ///
        /// </remarks>
        /// <param name="courseId">The course id.</param>
        /// <returns>The updated user course.</returns>
        /// <response code="200">Course saved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the tenant or course was not found.</response>
        [HttpPost("course/{courseId}/save")]
        public async Task<ActionResult<UserCourseDto>> SaveCourse(string courseId)
        {
            var command = new SaveUserCourse.Command
            {
                CourseId = courseId
            };

            return HandleResult(await Mediator.Send(command));
        }

        /// <summary>
        /// Marks a course as started for the current user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/user-courses/course/{courseId}/start
        ///
        /// </remarks>
        /// <param name="courseId">The course id.</param>
        /// <returns>The updated user course.</returns>
        /// <response code="200">Course started successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the tenant or course was not found.</response>
        [HttpPatch("course/{courseId}/start")]
        public async Task<ActionResult<UserCourseDto>> StartCourse(string courseId)
        {
            var command = new StartUserCourse.Command
            {
                CourseId = courseId
            };

            return HandleResult(await Mediator.Send(command));
        }

        /// <summary>
        /// Marks a course as completed for the current user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/user-courses/course/{courseId}/complete
        ///
        /// </remarks>
        /// <param name="courseId">The course id.</param>
        /// <returns>The updated user course.</returns>
        /// <response code="200">Course completed successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the tenant or course was not found.</response>
        [HttpPatch("course/{courseId}/complete")]
        public async Task<ActionResult<UserCourseDto>> CompleteCourse(string courseId)
        {
            var command = new CompleteUserCourse.Command
            {
                CourseId = courseId
            };

            return HandleResult(await Mediator.Send(command));
        }

        /// <summary>
        /// Removes a course from my saved list.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/user-courses/course/{courseId}/unsave
        ///
        /// </remarks>
        /// <param name="courseId">The course id.</param>
        /// <returns>The updated user course.</returns>
        /// <response code="200">Course unsaved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the tenant or user course was not found.</response>
        [HttpPatch("course/{courseId}/unsave")]
        public async Task<ActionResult<UserCourseDto>> UnsaveCourse(string courseId)
        {
            var command = new UnsaveUserCourse.Command
            {
                CourseId = courseId
            };

            return HandleResult(await Mediator.Send(command));
        }
    }
}
