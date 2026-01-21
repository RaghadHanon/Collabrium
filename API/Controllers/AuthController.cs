using API.Common.Attributes;
using API.Contracts.Identity;
using API.SetupProgram;
using Application.Features.Identity.Commands;
using Application.Features.Identity.DTOs;
using Application.Features.Users.DTOs;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Controller responsible for authentication-related operations.
/// </summary>
[Route("api/auth")]
public class AuthController : ApiBaseController
{
    /// <summary>
    /// Registers a new super admin.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/auth/superAdmin/registration
    ///     {
    ///         "fullName": "Gaby Millers",
    ///         "email": "gaby@test.com",
    ///         "password": "Pa$$w0rd",
    ///         "confirmPassword": "Pa$$w0rd",
    ///         "dateOfBirth": "2000-01-01",
    ///         "PhoneNumber": "0597644123"
    ///     }
    ///
    /// </remarks>
    /// <param name="registerSuperAdminRequest">The registration data for the new super admin.</param>
    /// <returns>The newly created super admin object.</returns>
    /// <response code="201">Returns the newly created user and a Location header pointing to it.</response>
    /// <response code="400">If the request data is invalid.</response>
    [Authorize(Policy = Policies.SuperAdmin)]
    [IgnoreTenantContext]
    [HttpPost("super-admin-registration")]
    public async Task<ActionResult<UserWithTenantDto>> RegisterSuperAdmin(RegisterSuperAdminRequest registerSuperAdminRequest)
    {
        var command = Mapper.Map<RegisterAppUser.Command>(registerSuperAdminRequest);
        command.UserRole = UserRole.SuperAdmin;

        return HandleResult(await Mediator.Send(command), "GetAppUser");
    }

    /// <summary>
    /// Registers a new app user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/auth/registration
    ///     {
    ///         "fullName": "Dana Holmes",
    ///         "email": "dana@test.com",
    ///         "password": "Pa$$w0rd",
    ///         "confirmPassword": "Pa$$w0rd",
    ///         "department": "SoftwareDevelopment",
    ///         "userRole": "BackendDev",
    ///         "dateOfBirth": "2000-01-01",
    ///         "PhoneNumber": "0597644123"
    ///     }
    ///
    /// </remarks>
    /// <param name="registerEmployeeRequest">The registration data for the new app user.</param>
    /// <returns>The newly created app user object.</returns>
    /// <response code="201">Returns the newly created user and a Location header pointing to it.</response>
    /// <response code="400">If the request data is invalid.</response>
    [Authorize(Policy = Policies.TenantAdmin)]
    [HttpPost("tenant-employee-registration")]
    public async Task<ActionResult<UserWithTenantDto>> RegisterTenantEmployee(RegisterEmployeeRequest registerEmployeeRequest)
    {
        var command = Mapper.Map<RegisterAppUser.Command>(registerEmployeeRequest);

        return HandleResult(await Mediator.Send(command), "GetAppUser");
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token upon successful login.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/auth/login
    ///     {
    ///         "email": "gaby@test.com",
    ///         "password": "Pa$$w0rd"
    ///     }
    ///
    /// </remarks>
    /// <param name="loginRequest"></param>
    /// <returns>The token and the user Id</returns>
    /// <response code="200">If the user logged in successfully</response>
    /// <response code="400">If the request data is invalid.</response>
    [IgnoreTenantContext]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequest loginRequest)
    {
        var command = Mapper.Map<LoginUser.Command>(loginRequest);

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Confirms a user's email using the provided token.
    /// </summary>
    /// <remarks>
    /// Sample request (JSON body):
    ///
    ///     POST /api/auth/confirm-email
    ///     {
    ///         "userId": "12345",
    ///         "token": "encoded-confirmation-token"
    ///     }
    ///
    /// </remarks>
    /// <param name="confirmEmailRequest">The confirm email data requested to be confirmed.</param>
    /// <returns>A success message if the email was confirmed successfully.</returns>
    /// <response code="200">If the email was confirmed successfully.</response>
    /// <response code="400">If the token is invalid or the request data is incorrect.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [IgnoreTenantContext]
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest confirmEmailRequest)
    {
        var command = Mapper.Map<ConfirmEmail.Command>(confirmEmailRequest);

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Resend the confirmation email to a user who has not confirmed their email yet.
    /// </summary>
    /// <param name="reSendConfirmationEmailRequest">The email of the user who needs the confirmation email.</param>
    /// <returns code="200">Returns a success message if the email was sent.</returns>
    /// <returns code="500">Returns an internal server error if things goes wrong.</returns>
    [IgnoreTenantContext]
    [HttpPost("resend-confirmation-email")]
    public async Task<IActionResult> ReSendConfirmationEmail(ReSendConfirmationEmailRequest reSendConfirmationEmailRequest)
    {
        var command = Mapper.Map<ReSendConfirmationEmail.Command>(reSendConfirmationEmailRequest);

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Sends a password reset email to the specified user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/send-password-reset
    ///     {
    ///         "email": "gaby@test.com"
    ///     }
    /// 
    /// </remarks>
    /// <param name="sendPasswordResetEmailRequest">The request containing the email of the user.</param>
    /// <returns>A success message if the email was sent.</returns>
    /// <response code="200">If the email was sent successfully.</response>
    /// <response code="400">If the email does not exist.</response>
    [IgnoreTenantContext]
    [HttpPost("send-password-reset-email")]
    public async Task<IActionResult> SendPasswordResetEmail(SendPasswordResetEmailRequest sendPasswordResetEmailRequest)
    {
        var command = Mapper.Map<SendPasswordResetEmail.Command>(sendPasswordResetEmailRequest);

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Resets the password for a user using a valid reset token.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/reset-password
    ///     {
    ///         "userId": "12345",
    ///         "token": "encoded-reset-token",
    ///         "newPassword": "NewPa$$w0rd"
    ///     }
    /// 
    /// </remarks>
    /// <param name="resetPasswordRequest">The request containing the userId, token, and new password.</param>
    /// <returns>A success message if the password was reset successfully.</returns>
    /// <response code="200">If the password was reset successfully.</response>
    /// <response code="400">If the reset token is invalid or the request data is incorrect.</response>
    [IgnoreTenantContext]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
    {
        var command = Mapper.Map<ResetPassword.Command>(resetPasswordRequest);

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Changes the password for a logged-in user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/change-password
    ///     {
    ///         "currentPassword": "OldPa$$w0rd",
    ///         "newPassword": "NewPa$$w0rd"
    ///     }
    /// 
    /// </remarks>
    /// <param name="changePasswordRequest">The request containing the userId, current password, and new password.</param>
    /// <returns>A success message if the password was changed successfully.</returns>
    /// <response code="200">If the password was changed successfully.</response>
    /// <response code="400">If the current password is incorrect or the request data is invalid.</response>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
    {
        var command = Mapper.Map<ChangePassword.Command>(changePasswordRequest);

        return HandleResult(await Mediator.Send(command));
    }
}