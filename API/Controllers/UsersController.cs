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
    ///     GET /api/users/me
    /// 
    /// </remarks>
    /// <returns>The details of the current user.</returns>
    /// <response code="200">Returns the user details successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user has not changed their initial password.</response>
    [HttpGet("users/me")]
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
    ///     GET /api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the user to retrieve.</param>
    /// <returns>The details of the specified user.</returns>
    /// <response code="200">Returns the user details successfully.</response>
    /// <response code="404">If the user with the specified ID was not found.</response>
    /// <response code="403">If the requesting user has not changed their initial password.</response>
    [HttpGet("users/{id}")]
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
    ///     PUT /api/users/user-profile
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
    ///     Content-Type: multipart/form-data
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
}
