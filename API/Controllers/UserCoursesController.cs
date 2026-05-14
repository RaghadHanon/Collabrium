using API.Contracts.UserCourses;
using API.SetupProgram;
using Application.Common;
using Application.Features.UserCourses.Commands;
using Application.Features.UserCourses.DTOs;
using Application.Features.UserCourses.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

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
    ///     GET /api/user-courses?search=sql&amp;status=InProgress&amp;isSaved=true&amp;isAssigned=false&amp;pageSize=10
    ///
    /// </remarks>
    /// <param name="request">Filtering and pagination parameters.</param>
    /// <returns>Paged list of my user courses.</returns>
    /// <response code="200">User courses retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read courses.</response>
    /// <response code="404">If the tenant was not found.</response>
    [Authorize(Policy = Policies.Courses_Read)]
    [HttpGet]
    public async Task<ActionResult<PagedList<UserCourseDto, DateTime?>>> GetMyCourses(
        [FromQuery] GetMyUserCoursesRequest request)
    {
        var query = Mapper.Map<GetMyUserCourses.Query>(request);
        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves my saved courses only.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/user-courses/saved?search=backend&amp;pageSize=10
    ///
    /// </remarks>
    /// <param name="request">Filtering and pagination parameters.</param>
    /// <returns>Paged list of my saved courses.</returns>
    /// <response code="200">Saved courses retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read courses.</response>
    /// <response code="404">If the tenant was not found.</response>
    [Authorize(Policy = Policies.Courses_Read)]
    [HttpGet("saved")]
    public async Task<ActionResult<PagedList<UserCourseDto, DateTime?>>> GetMySavedCourses(
        [FromQuery] GetMySavedUserCoursesRequest request)
    {
        var query = Mapper.Map<GetMySavedCourses.Query>(request);
        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves courses assigned to me.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/user-courses/assigned-to-me?search=dotnet&amp;status=InProgress&amp;pageSize=10
    ///
    /// </remarks>
    /// <param name="request">Filtering and pagination parameters.</param>
    /// <returns>Paged list of assigned courses.</returns>
    /// <response code="200">Assigned courses retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read courses.</response>
    /// <response code="404">If the tenant was not found.</response>
    [Authorize(Policy = Policies.Courses_Read)]
    [HttpGet("assigned-to-me")]
    public async Task<ActionResult<PagedList<UserCourseDto, DateTime?>>> GetAssignedToMeCourses(
        [FromQuery] GetAssignedToMeCoursesRequest request)
    {
        var query = Mapper.Map<GetAssignedToMeCourses.Query>(request);
        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves my user-course record by course id.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/user-courses/course/{courseId}
    ///
    /// </remarks>
    /// <param name="courseId">The course id.</param>
    /// <returns>The user-course record for the current user and this course.</returns>
    /// <response code="200">User course retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read courses.</response>
    /// <response code="404">If the tenant or user course was not found.</response>
    [Authorize(Policy = Policies.Courses_Read)]
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
    /// Retrieves user-course details by user-course id.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/user-courses/{userCourseId}
    ///
    /// </remarks>
    /// <param name="userCourseId">The user-course id.</param>
    /// <returns>User-course details.</returns>
    /// <response code="200">User-course details retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read courses.</response>
    /// <response code="404">If the tenant or user course was not found.</response>
    [Authorize(Policy = Policies.Courses_Read)]
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
    /// <returns>The updated user-course record.</returns>
    /// <response code="200">Course saved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read courses.</response>
    /// <response code="404">If the tenant or course was not found.</response>
    [Authorize(Policy = Policies.Courses_Read)]
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
    /// Removes a course from my saved list.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/user-courses/course/{courseId}/unsave
    ///
    /// </remarks>
    /// <param name="courseId">The course id.</param>
    /// <returns>The updated user-course record.</returns>
    /// <response code="200">Course unsaved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read courses.</response>
    /// <response code="404">If the tenant, course, or user-course was not found.</response>
    [Authorize(Policy = Policies.Courses_Read)]
    [HttpPatch("course/{courseId}/unsave")]
    public async Task<ActionResult<UserCourseDto>> UnsaveCourse(string courseId)
    {
        var command = new UnsaveUserCourse.Command
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
    /// <returns>The updated user-course record.</returns>
    /// <response code="200">Course started successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read courses.</response>
    /// <response code="404">If the tenant or course was not found.</response>
    [Authorize(Policy = Policies.Courses_Read)]
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
    /// <returns>The updated user-course record.</returns>
    /// <response code="200">Course completed successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have permission to read courses.</response>
    /// <response code="404">If the tenant or course was not found.</response>
    [Authorize(Policy = Policies.Courses_Read)]
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
    /// Retrieves summary counts for the current user's courses.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/user-courses/my-summary
    ///
    /// </remarks>
    /// <returns>Counts of all/saved/assigned/in-progress/completed courses for the current user.</returns>
    /// <response code="200">Summary retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If tenant not found.</response>
    [HttpGet("my-summary")]
    public async Task<ActionResult<MyLearningSummaryDto>> GetMySummary()
    {
        var query = new GetMyUserCoursesSummary.Query();
        return HandleResult(await Mediator.Send(query));
    }
    /// <summary>
    /// Retrieves stats for courses assigned by the current user.
    /// </summary>
    [HttpGet("assigned-by-me-stats")]
    public async Task<ActionResult<AssignedByMeStatsDto>> GetAssignedByMeStats()
    {
        var query = new GetAssignedByMeStats.Query();
        return HandleResult(await Mediator.Send(query));
    }
}
