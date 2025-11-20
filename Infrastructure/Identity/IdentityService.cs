using Application.Common.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Emails;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Email;
using Infrastructure.Persistence;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Text;

namespace Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly AppDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly IdentitySettings _identitySettings;
    private readonly IEmailTemplateFactory _emailTemplateFactory;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator,
        AppDbContext dbContext,
        IEmailService emailService,
        IOptions<IdentitySettings> identityOptions,
        IEmailTemplateFactory emailTemplateFactory)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _dbContext = dbContext;
        _emailService = emailService;
        _identitySettings = identityOptions.Value;
        _emailTemplateFactory = emailTemplateFactory;
    }

    public async Task<ApplicationUser> RegisterUserAsync(
        string fullName,
        Department department,
        string password,
        DateOnly dateOfBirth,
        UserRole role,
        string tenantId,
        string? email = null,
        string? phoneNumber = null)
    {
        var user = CreateApplicationUser(fullName, department, dateOfBirth, tenantId, email, phoneNumber);

        await CreateUserAsync(user, password);
        await AssignRoleAsync(user, role);
        await SendConfirmationEmail(user);

        return user;
    }

    private async Task CreateUserAsync(ApplicationUser user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded) return;

        var errors = string.Join(" ", result.Errors.Select(e => e.Description));
        throw new BadRequestException($"User creation failed: {errors}");
    }

    private async Task AssignRoleAsync(ApplicationUser user, UserRole role)
    {
        var roleName = role.ToString();

        if (!await _roleManager.RoleExistsAsync(roleName))
            throw new BadRequestException($"Role '{roleName}' does not exist.");

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Failed to assign role '{roleName}': {errors}");
        }
    }

    public async Task SendConfirmationEmail(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmationUrl = $"{_identitySettings.ConfirmationUrlBase}?userId={user.Id}&token={encodedToken}";

        var template = _emailTemplateFactory.GetTemplate("Confirmation");

        var data = new Dictionary<string, string>
        {
            ["FullName"] = user.FullName,
            ["Url"] = confirmationUrl
        };

        var message = new EmailMessageBuilder()
            .To(user.Email!)
            .Subject(template.GetSubject())
            .PlainTextBody(template.GetPlainTextBody(data))
            .HtmlBody(template.GetHtmlBody(data))
            .Build();

        await _emailService.SendAsync(message, cancellationToken);
    }

    public async Task ReSendConfirmationEmail(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new BadRequestException("User not found.");

        if (await _userManager.IsEmailConfirmedAsync(user))
            throw new BadRequestException("Email is already confirmed.");

        await SendConfirmationEmail(user, cancellationToken);
    }

    private ApplicationUser CreateApplicationUser(
        string fullName,
        Department department,
        DateOnly dateOfBirth,
        string tenantId,
        string? email = null,
        string? phoneNumber = null)
    {
        return new ApplicationUser
        {
            FullName = fullName,
            Email = email,
            TenantId = tenantId,
            UserName = email,
            DateOfBirth = dateOfBirth,
            PhoneNumber = phoneNumber
        };
    }

    public async Task<(string Token, string Id)> AuthenticateAsync(string email, string password)
    {
        var user = await ValidateCredentialsAsync(email, password);
        var roles = await GetRolesAsync(user);

        var token = _jwtTokenGenerator.GenerateToken(user, roles)
                    ?? throw new TokenGenerationFailedException("Token generation failed");
        var entityId = user.Id;

        return (token, entityId);
    }

    private async Task<ApplicationUser> ValidateCredentialsAsync(string email, string password)
    {
        var user = await GetUserAsync(email);
        var validPassword = await _userManager.CheckPasswordAsync(user!, password);

        return !validPassword ? throw new UnauthenticatedException("Invalid password.") : user!;
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return roles.Count == 0 ? throw new NoRolesException($"User '{user.Email}' has no roles assigned.") : roles;
    }

    public async Task<ApplicationUser?> GetUserAsync(string email)
    {
        return email != null
            ? await _userManager.FindByEmailAsync(email)
                ?? throw new UnauthenticatedException("Invalid email.")
            : throw new BadRequestException("Email must be provided.");
    }

    public async Task UpdateUserAsync(ApplicationUser user)
    {
        await _userManager.UpdateAsync(user);
    }

    public async Task SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new BadRequestException("User not found.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetUrl = $"{_identitySettings.PasswordResetUrlBase}?userId={user.Id}&token={encodedToken}";

        var template = _emailTemplateFactory.GetTemplate("PasswordReset");

        var data = new Dictionary<string, string>
        {
            ["FullName"] = user.FullName,
            ["Url"] = resetUrl
        };

        var message = new EmailMessageBuilder()
            .To(user.Email!)
            .Subject(template.GetSubject())
            .PlainTextBody(template.GetPlainTextBody(data))
            .HtmlBody(template.GetHtmlBody(data))
            .Build();

        await _emailService.SendAsync(message, cancellationToken);
    }

    public async Task ResetPasswordAsync(string userId, string token, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new BadRequestException("User not found.");

        user.MustChangePassword = false;
        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Password reset failed: {errors}");
        }
    }

    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new BadRequestException("User not found.");

        user.MustChangePassword = false;
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Password change failed: {errors}");
        }
    }

    public async Task<bool> IsEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        return await _userManager.IsEmailConfirmedAsync(user);
    }
}