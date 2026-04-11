using API.Contracts.CourseRatings;
using API.SetupProgram;
using Application.Features.CourseRatings.Commands;
using Application.Features.CourseRatings.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/courses/{courseId}/ratings")]
    public class CourseRatingsController : ApiBaseController
    {
        /// <summary>
        /// Add or update rating for a course (current user)
        /// </summary>
        /// <param name="courseId">Course Id</param>
        /// <param name="request">Rating data</param>
        /// <returns>Course rating</returns>
        /// <response code="200">Rating added or updated successfully</response>
        /// <response code="404">Course not found</response>
        [Authorize(Policy = Policies.Courses_Read)]
        [HttpPost]
        public async Task<ActionResult> RateCourse(
            string courseId,
            RateCourseRequest request)
        {
            var command = Mapper.Map<RateCourse.Command>(request);
            command.CourseId = courseId;

            return HandleResult(await Mediator.Send(command));
        }

        /// <summary>
        /// Get all ratings for a course
        /// </summary>
        /// <param name="courseId">Course Id</param>
        /// <param name="request">Filter & pagination</param>
        /// <returns>Paged list of ratings</returns>
        /// <response code="200">Returns ratings</response>
        /// <response code="404">Course not found</response>
        [Authorize(Policy = Policies.Courses_Manage)]
        [HttpGet]
        public async Task<ActionResult> GetCourseRatings(
            string courseId,
            [FromQuery] GetCourseRatingsRequest request)
        {
            var query = Mapper.Map<GetCourseRatings.Query>(request);
            query.CourseId = courseId;

            return HandleResult(await Mediator.Send(query));
        }

        /// <summary>
        /// Get current user's rating for a course
        /// </summary>
        /// <param name="courseId">Course Id</param>
        /// <returns>User rating</returns>
        /// <response code="200">Returns rating</response>
        /// <response code="404">Rating not found</response>
        [Authorize(Policy = Policies.Courses_Read)]
        [HttpGet("me")]
        public async Task<ActionResult> GetMyRating(string courseId)
        {
            var query = new GetMyCourseRating.Query
            {
                CourseId = courseId
            };

            return HandleResult(await Mediator.Send(query));
        }
    }
}
