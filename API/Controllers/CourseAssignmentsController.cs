using API.Contracts.CourseAssignments;
using API.SetupProgram;
using Application.Common;
using Application.Features.CourseAssignments.Commands;
using Application.Features.CourseAssignments.DTOs;
using Application.Features.CourseAssignments.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/course-assignments")]
    [ApiController]
    [Authorize(Policy = Policies.PasswordChanged)]
    public sealed class CourseAssignmentsController : ApiBaseController
    {
        // ===============================
        // Assign
        // ===============================

        /// <summary>
        /// Assign a course to a user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/course-assignments/course/{courseId}/assign-user
        ///
        ///     {
        ///         "assignedToUserId": "user-id"
        ///     }
        ///
        /// </remarks>
        /// <param name="courseId">The course id from route.</param>
        /// <param name="request">Assignment payload.</param>
        /// <returns>The created course assignment.</returns>
        /// <response code="201">Course assigned successfully.</response>
        /// <response code="400">If request is invalid or assignment already exists.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If course not found.</response>
        [HttpPost("course/{courseId}/assign-user")]
        public async Task<ActionResult<CourseAssignmentDto>> AssignCourseToUser(
            string courseId,
            AssignCourseToUserRequest request)
        {
            var command = Mapper.Map<AssignCourseToUser.Command>(request);
            command.CourseId = courseId;

            return HandleResult(
                await Mediator.Send(command),
                nameof(GetCourseAssignment),
                "CourseAssignments");
        }

        // ===============================
        // Status Actions
        // ===============================

        /// <summary>
        /// Mark a course assignment as completed.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/course-assignments/{assignmentId}/complete
        ///
        /// </remarks>
        /// <param name="assignmentId">The assignment id.</param>
        /// <returns>The updated assignment.</returns>
        /// <response code="200">Assignment completed successfully.</response>
        /// <response code="400">If assignment cannot be completed.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If assignment not found.</response>
        [HttpPatch("{assignmentId}/complete")]
        public async Task<ActionResult<CourseAssignmentDto>> CompleteCourseAssignment(string assignmentId)
        {
            var command = new CompleteCourseAssignment.Command
            {
                AssignmentId = assignmentId
            };

            return HandleResult(await Mediator.Send(command));
        }

        /// <summary>
        /// Cancel a course assignment.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/course-assignments/{assignmentId}/cancel
        ///
        /// </remarks>
        /// <param name="assignmentId">The assignment id.</param>
        /// <returns>The updated assignment.</returns>
        /// <response code="200">Assignment cancelled successfully.</response>
        /// <response code="400">If assignment cannot be cancelled.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If assignment not found.</response>
        [HttpPatch("{assignmentId}/cancel")]
        public async Task<ActionResult<CourseAssignmentDto>> CancelCourseAssignment(string assignmentId)
        {
            var command = new CancelCourseAssignment.Command
            {
                AssignmentId = assignmentId
            };

            return HandleResult(await Mediator.Send(command));
        }

        /// <summary>
        /// Reactivate a cancelled course assignment.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/course-assignments/{assignmentId}/reactivate
        ///
        /// </remarks>
        /// <param name="assignmentId">The assignment id.</param>
        /// <returns>The updated assignment.</returns>
        /// <response code="200">Assignment reactivated successfully.</response>
        /// <response code="400">If assignment cannot be reactivated.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If assignment not found.</response>
        [HttpPatch("{assignmentId}/reactivate")]
        public async Task<ActionResult<CourseAssignmentDto>> ReactivateCourseAssignment(string assignmentId)
        {
            var command = new ReactivateCourseAssignment.Command
            {
                AssignmentId = assignmentId
            };

            return HandleResult(await Mediator.Send(command));
        }

        // ===============================
        // Queries
        // ===============================

        /// <summary>
        /// Get a course assignment by id.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/course-assignments/{assignmentId}
        ///
        /// </remarks>
        /// <param name="assignmentId">The assignment id.</param>
        /// <returns>The assignment details.</returns>
        /// <response code="200">Assignment retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If assignment not found.</response>
        [HttpGet("{assignmentId}")]
        public async Task<ActionResult<CourseAssignmentDetailsDto>> GetCourseAssignment(string assignmentId)
        {
            var query = new GetCourseAssignment.Query
            {
                AssignmentId = assignmentId
            };

            return HandleResult(await Mediator.Send(query));
        }

        /// <summary>
        /// Get all course assignments with filtering and pagination.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/course-assignments?courseId=...&status=Active
        ///
        /// </remarks>
        /// <param name="request">Filtering and pagination parameters.</param>
        /// <returns>Paged list of assignments.</returns>
        /// <response code="200">Assignments retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet]
        public async Task<ActionResult<PagedList<CourseAssignmentDto, DateTime?>>> GetCourseAssignments(
            [FromQuery] GetCourseAssignmentsRequest request)
        {
            var query = Mapper.Map<GetCourseAssignments.Query>(request);

            return HandleResult(await Mediator.Send(query));
        }

        /// <summary>
        /// Get assignments for the current user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/course-assignments/my
        ///
        /// </remarks>
        /// <returns>List of user's assignments.</returns>
        /// <response code="200">Assignments retrieved successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet("my")]
        public async Task<ActionResult<List<CourseAssignmentDto>>> GetMyAssignments()
        {
            var query = new GetMyCourseAssignments.Query();

            return HandleResult(await Mediator.Send(query));
        }

        // ===============================
        // Delete
        // ===============================

        /// <summary>
        /// Delete a course assignment.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /api/course-assignments/{assignmentId}
        ///
        /// </remarks>
        /// <param name="assignmentId">The assignment id.</param>
        /// <response code="200">Assignment deleted successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If assignment not found.</response>
        [HttpDelete("{assignmentId}")]
        public async Task<ActionResult> DeleteCourseAssignment(string assignmentId)
        {
            var command = new DeleteCourseAssignment.Command
            {
                AssignmentId = assignmentId
            };

            return HandleResult(await Mediator.Send(command));
        }
    }
}

