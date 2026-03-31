using API.Contracts.Courses;
using Application.Features.Courses.Commands;
using Application.Features.Courses.DTOs;
using Application.Features.Courses.Queries;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/courses")]
    [ApiController]
    [Authorize]
    public sealed class CoursesController : ApiBaseController
    {
        private readonly IMapper _mapper;

        public CoursesController(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new course within the current tenant.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/courses/course
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "title": "ASP.NET Core Fundamentals",
        ///       "description": "A complete introduction to ASP.NET Core.",
        ///       "url": "https://example.com/courses/aspnet-core-fundamentals",
        ///       "provider": "Udemy",
        ///       "instructorName": "John Doe",
        ///       "level": "Junior",
        ///       "language": "English",
        ///       "estimatedDurationInHours": 12,
        ///       "tagIds": ["tag1", "tag2"]
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Course created successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage courses.</response>
        /// <response code="404">If the current tenant is not found.</response>
        [HttpPost("course")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
        {
            var command = _mapper.Map<CreateCourse.Command>(request);
            var result = await Mediator.Send(command);
            return HandleResult(result, nameof(GetCourseById), "Courses");
        }

        /// <summary>
        /// Retrieves a course by id (scoped to current tenant).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/courses/course/{courseId}
        ///
        /// </remarks>
        /// <response code="200">Course retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to read courses.</response>
        /// <response code="404">If the current tenant is not found or the course does not exist.</response>
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseById([FromRoute] string courseId)
        {
            var query = new GetCourse.Query
            {
                CourseId = courseId
            };

            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves courses with filtering and cursor-based pagination.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/courses?search=ASP.NET&amp;pageSize=10
        ///
        /// Cursor pagination:
        ///
        ///     GET /api/courses?cursor=2026-01-01T00:00:00Z&amp;pageSize=10
        ///
        /// </remarks>
        /// <response code="200">Courses retrieved successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to read courses.</response>
        /// <response code="404">If the current tenant is not found.</response>
        [HttpGet]
        public async Task<IActionResult> GetCourses([FromQuery] GetCoursesRequest request)
        {
            var query = _mapper.Map<GetCourses.Query>(request);
            var result = await Mediator.Send(query);

            return HandleResult(result);
        }

        /// <summary>
        /// Updates course details.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/courses/course/{courseId}
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "title": "Updated Course Title",
        ///       "description": "Updated course description",
        ///       "provider": "Coursera"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Course updated successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage courses.</response>
        /// <response code="404">If the current tenant is not found or the course does not exist.</response>
        [HttpPatch("course/{courseId}")]
        public async Task<IActionResult> UpdateCourse([FromRoute] string courseId, [FromBody] UpdateCourseRequest request)
        {
            var command = _mapper.Map<UpdateCourse.Command>(request);
            command.CourseId = courseId;

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Deletes a course.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /api/courses/course/{courseId}
        ///
        /// </remarks>
        /// <response code="204">Course deleted successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage courses.</response>
        /// <response code="404">If the current tenant is not found or the course does not exist.</response>
        [HttpDelete("course/{courseId}")]
        public async Task<IActionResult> DeleteCourse([FromRoute] string courseId)
        {
            var command = new DeleteCourse.Command
            {
                CourseId = courseId
            };

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Sets course tags by replacing all existing tags.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/courses/course/{courseId}/tags
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "tagIds": ["tag1", "tag2"]
        ///     }
        ///
        /// Note:
        /// Sending an empty array will remove all existing tags.
        ///
        /// </remarks>
        /// <response code="200">Course tags updated successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage courses.</response>
        /// <response code="404">If the current tenant, course, or one of the tags does not exist.</response>
        [HttpPut("course/{courseId}/tags")]
        public async Task<IActionResult> SetCourseTags([FromRoute] string courseId, [FromBody] SetCourseTagsRequest request)
        {
            var command = new SetCourseTags.Command
            {
                CourseId = courseId,
                TagIds = request.TagIds
            };

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Adds tags to a course.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/courses/course/{courseId}/tags
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "tagIds": ["tag1", "tag2"]
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Tags added successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage courses.</response>
        /// <response code="404">If the current tenant, course, or one of the tags does not exist.</response>
        [HttpPost("course/{courseId}/tags")]
        public async Task<IActionResult> AddCourseTags([FromRoute] string courseId, [FromBody] AddCourseTagsRequest request)
        {
            var command = new AddCourseTags.Command
            {
                CourseId = courseId,
                TagIds = request.TagIds
            };

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Removes tags from a course.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/courses/course/{courseId}/tags
        ///     Content-Type: application/json
        ///
        ///     {
        ///       "tagIds": ["tag1", "tag2"]
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Tags removed successfully.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage courses.</response>
        /// <response code="404">If the current tenant, course, or one of the tags does not exist.</response>
        [HttpPatch("course/{courseId}/tags")]
        public async Task<IActionResult> RemoveCourseTags([FromRoute] string courseId, [FromBody] RemoveCourseTagsRequest request)
        {
            var command = new RemoveCourseTags.Command
            {
                CourseId = courseId,
                TagIds = request.TagIds
            };

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Uploads a thumbnail image for a course.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/courses/course/{courseId}/thumbnail
        ///     Content-Type: multipart/form-data
        ///
        ///     file: [binary image file]
        ///
        /// </remarks>
        /// <response code="204">Thumbnail uploaded successfully.</response>
        /// <response code="400">If the request data is invalid or no file was uploaded.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission to manage courses.</response>
        /// <response code="404">If the current tenant or course does not exist.</response>
        [HttpPost("course/{courseId}/thumbnail")]
        public async Task<IActionResult> UploadCourseThumbnail([FromRoute] string courseId, [FromForm] UploadCourseThumbnailRequest request)
        {
            var command = _mapper.Map<UploadCourseThumbnail.Command>(request);
            command.CourseId = courseId;

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        [HttpGet("{courseId}/tags")]
        public async Task<ActionResult<List<CourseTagDto>>> GetCourseTags(string courseId)
        {
            var query = new GetCourseTags.Query
            {
                CourseId = courseId
            };

            return HandleResult(await Mediator.Send(query));
        }
    }
}

