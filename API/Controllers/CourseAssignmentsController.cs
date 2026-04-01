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
        [HttpPost("course/{courseId}/assign-user")]
        public async Task<ActionResult<CourseAssignmentDto>> AssignCourseToUser(
            string courseId,
            AssignCourseToUserRequest request)
        {
            var command = Mapper.Map<AssignCourseToUser.Command>(request);
            command.CourseId = courseId;

            return HandleResult(await Mediator.Send(command));
        }

        // ===============================
        // Status Actions
        // ===============================

        /// <summary>
        /// Mark assignment as completed.
        /// </summary>
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
        /// Cancel assignment.
        /// </summary>
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
        /// Reactivate assignment.
        /// </summary>
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
        /// Get assignment by id.
        /// </summary>
        [HttpGet("{assignmentId}")]
        public async Task<ActionResult<CourseAssignmentDto>> GetCourseAssignment(string assignmentId)
        {
            var query = new GetCourseAssignment.Query
            {
                AssignmentId = assignmentId
            };

            return HandleResult(await Mediator.Send(query));
        }

        /// <summary>
        /// Get all assignments (paged + filtered).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedList<CourseAssignmentDto, DateTime?>>> GetCourseAssignments(
            [FromQuery] GetCourseAssignmentsRequest request)
        {
            var query = Mapper.Map<GetCourseAssignments.Query>(request);

            return HandleResult(await Mediator.Send(query));
        }

        /// <summary>
        /// Get current user's assignments.
        /// </summary>
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
        /// Delete assignment.
        /// </summary>
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

