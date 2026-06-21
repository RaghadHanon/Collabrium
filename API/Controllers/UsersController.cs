using API.Common.Attributes;
using API.Contracts.Tenants;
using API.Contracts.Users;
using API.SetupProgram;
using Application.Features.Users.Commands;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/users")]
[Authorize(Policy = Policies.PasswordChanged)]
public class UsersController : ApiBaseController
{
    /// <summary>
    /// Retrieves the profile of the currently logged-in user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/users/user/me
    /// 
    /// </remarks>
    /// <returns>The details of the current user.</returns>
    /// <response code="200">Returns the user details successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user has not changed their initial password.</response>
    [HttpGet("user/me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var query = new GetUser.Query();

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the details of a specific user by their ID.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/users/user/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the user to retrieve.</param>
    /// <returns>The details of the specified user.</returns>
    /// <response code="200">Returns the user details successfully.</response>
    /// <response code="404">If the user with the specified ID was not found.</response>
    /// <response code="403">If the requesting user has not changed their initial password.</response>
    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var query = new GetUser.Query();
        query.UserId = id;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Updates the profile information of the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/users/user-profile
    ///     {
    ///         "firstName": "John",
    ///         "lastName": "Doe",
    ///         "dateOfBirth": "1990-01-01",
    ///         "phoneNumber": "0597612345",
    ///         "linkedIn": "https://linkedin.com/in/johndoe",
    ///         "facebook": "https://facebook.com/johndoe",
    ///         "instagram": "https://instagram.com/johndoe",
    ///         "youTube": "https://youtube.com/johndoe",
    ///         "github": "https://github.com/johndoe"
    ///     }
    ///
    /// Only the fields provided in the request will be updated. Fields left null will remain unchanged.
    /// </remarks>
    /// <param name="request">The updated user profile information.</param>
    /// <returns>The updated user details along with associated tenant information.</returns>
    /// <response code="200">User profile updated successfully, returning the updated user data.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPatch("user-profile")]
    public async Task<ActionResult<UserWithTenantDto>> UpdateUserProfile(UpdateUserProfileRequest request)
    {
        var command = Mapper.Map<UpdateUserProfile.Command>(request);

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Uploads or updates the profile image for the current user.
    /// </summary>
    /// <remarks>
    /// This endpoint accepts a file upload via `multipart/form-data`.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/users/profile-image
    ///     Content-RelationType: multipart/form-data
    ///     
    ///     Form Data:
    ///     key: "file", value: [Select image file], type: File
    /// 
    /// </remarks>
    /// <param name="request">The image file to upload (e.g., PNG, JPG, JPEG).</param>
    /// <returns>A success status if the upload completes.</returns>
    /// <response code="200">If the image was uploaded successfully.</response>
    /// <response code="400">If no file is provided or the file format is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    [RejectUnknownFormFields(typeof(UploadImageRequest), allowEmptyRequest: false)]
    [HttpPost("profile-image")]
    public async Task<ActionResult> UploadProfileImage([FromForm] UploadImageRequest request)
    {
        var command = Mapper.Map<UploadUserProfileImage.Command>(request);

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Updates the role of a specific user.
    /// </summary>
    /// <remarks>
    /// This endpoint allows an authorized administrator to replace the user's current role
    /// with a new one. The user can only have one role at a time.
    ///
    /// Sample request:
    ///
    ///     PATCH /api/users/{id}/user-role
    ///     {
    ///         "newRole": "TeamLead"
    ///     }
    ///
    /// </remarks>
    /// <param name="id">The Id of the user.</param>
    /// <param name="request">The target user ID and the new role to assign.</param>
    /// <returns>No[[ content if the role was updated successfully.</returns>
    /// <response code="204">User role updated successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized to update roles.</response>
    /// <response code="404">If the target user was not found.</response>
    [Authorize(Policy = Policies.TenantAdmin)]
    [HttpPatch("{id}/user-role")]
    public async Task<IActionResult> UpdateUserRole(string id, UpdateUserRoleRequest request)
    {
        var command = Mapper.Map<UpdateUserRole.Command>(request);
        command.UserId = id;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Get users that can be assigned (exclude managers + current user)
    /// </summary>
    [HttpGet("assignable-users")]
    public async Task<IActionResult> GetAssignableUsers(
        [FromQuery] GetAssignableUsers.Query query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Adds skills to the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// This endpoint allows the current user to add one or more skills to their own profile.
    /// Existing skills will remain unchanged, and duplicate skills will be ignored.
    ///
    /// Sample request:
    ///
    ///     POST /api/users/me/skills
    ///     {
    ///         "skillIds": [1, 2, 3]
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The skill IDs to add to the current user's profile.</param>
    /// <returns>No content if the skills were added successfully.</returns>
    /// <response code="204">Skills added successfully.</response>
    /// <response code="400">If the request is invalid or no skills are provided.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user has not changed their initial password.</response>
    /// <response code="404">If the current user was not found.</response>
    [HttpPost("me/skills")]
    public async Task<IActionResult> AddMySkills(AddUserSkillsRequest request)
    {
        var command = Mapper.Map<AddUserSkills.Command>(request);

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Retrieves the skills of the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/users/me/skills
    ///
    /// </remarks>
    /// <returns>The list of skills assigned to the current user.</returns>
    /// <response code="200">Returns the current user's skills successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user has not changed their initial password.</response>
    /// <response code="404">If the current user was not found.</response>
    [HttpGet("me/skills")]
    public async Task<IActionResult> GetMySkills()
    {
        var query = new GetUserSkills.Query();

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the skills of a specific user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/skills
    ///
    /// </remarks>
    /// <param name="id">The Id of the user whose skills should be retrieved.</param>
    /// <returns>The list of skills assigned to the specified user.</returns>
    /// <response code="200">Returns the user's skills successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized to view the specified user's skills.</response>
    /// <response code="404">If the target user was not found.</response>
    [HttpGet("{id}/skills")]
    public async Task<IActionResult> GetUserSkills(string id)
    {
        var query = new GetUserSkills.Query
        {
            UserId = id
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Deletes skills from the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// This endpoint removes one or more skills from the current user's profile.
    ///
    /// Sample request:
    ///
    ///     DELETE /api/users/me/skills
    ///     {
    ///         "skillIds": [1, 2, 3]
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The skill IDs to remove from the current user's profile.</param>
    /// <returns>No content if the skills were removed successfully.</returns>
    /// <response code="204">Skills deleted successfully.</response>
    /// <response code="400">If the request is invalid or no skills are provided.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user has not changed their initial password.</response>
    /// <response code="404">If the current user was not found.</response>
    [HttpDelete("me/skills")]
    public async Task<IActionResult> DeleteMySkills(DeleteUserSkillsRequest request)
    {
        var command = Mapper.Map<DeleteUserSkills.Command>(request);

        return HandleResult(await Mediator.Send(command));
    }

    [HttpGet("managers")]
    public async Task<ActionResult<List<UserDto>>> GetManagers()
    {
        return HandleResult(await Mediator.Send(new GetManagers.Query()));
    }


    [HttpPatch("{userId}/toggle-activation")]
    [Authorize(Policy = Policies.Local_Tenant_Manage)]
    public async Task<IActionResult> ToggleUserActivation(
        string userId,
        ToggleUserActivationRequest request,
        CancellationToken cancellationToken)
    {
        var command = Mapper.Map<ToggleUserActivation.Command>(request);
        command.UserId = userId;

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}