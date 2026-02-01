using Application.Common.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Emails;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Email;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mapping;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Text;

namespace Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUserEntity> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;
    private readonly IdentitySettings _identitySettings;
    private readonly IEmailTemplateFactory _emailTemplateFactory;

    public IdentityService(
        UserManager<ApplicationUserEntity> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator,
        AppDbContext dbContext,
        IEmailService emailService,
        IOptions<IdentitySettings> identityOptions,
        IEmailTemplateFactory emailTemplateFactory)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _identitySettings = identityOptions?.Value ?? throw new ArgumentNullException(nameof(identityOptions));
        _emailTemplateFactory = emailTemplateFactory ?? throw new ArgumentNullException(nameof(emailTemplateFactory));
    }

    public async Task<User> RegisterUserAsync(
    string fullName,
    Department department,
    string password,
    DateOnly dateOfBirth,
    UserRole role,
    string tenantId,
    string email,
    string phoneNumber,
    CancellationToken cancellationToken = default)
    {
        var userEntity = CreateApplicationUserEntity(fullName, department, dateOfBirth, tenantId, email, phoneNumber);
        userEntity.MustChangePassword = role != UserRole.SuperAdmin;

        await CreateUserAsync(userEntity, password);
        await AssignRoleAsync(userEntity, role);

        try
        {
            await SendConfirmationEmailAsync(userEntity, cancellationToken);
        }
        catch
        {
        }

        return UserMapper.ToDomain(userEntity);
    }

    public async Task<(string Token, string Id, string TenantId)> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var userEntity = await ValidateCredentialsAsync(email, password);
        if (!await _userManager.IsEmailConfirmedAsync(userEntity))
            throw new UnauthenticatedException("You must confirm your email before logging in.");

        var roles = await GetRolesAsync(userEntity);
        userEntity.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(userEntity);

        var token = _jwtTokenGenerator.GenerateToken(UserMapper.ToDomain(userEntity), roles);

        return string.IsNullOrWhiteSpace(token)
            ? throw new TokenGenerationFailedException("Token generation failed.")
            : ((string Token, string Id, string TenantId)) (token, userEntity.Id, userEntity.TenantId ?? string.Empty);
    }

    public async Task ConfirmEmailAsync(string userId, string encodedToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new BadRequestException("UserId must be provided.");

        if (string.IsNullOrWhiteSpace(encodedToken))
            throw new BadRequestException("Token must be provided.");

        var user = await _userManager.FindByIdAsync(userId.Trim());
        if (user is null)
            throw new BadRequestException("User not found.");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Email confirmation failed: {errors}");
        }
    }

    public async Task ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Email must be provided.");

        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null)
            throw new BadRequestException("User not found.");

        if (await _userManager.IsEmailConfirmedAsync(user))
            throw new BadRequestException("Email is already confirmed.");

        await SendConfirmationEmailAsync(user, cancellationToken);
    }

    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new BadRequestException("UserId must be provided.");

        var user = await _userManager.FindByIdAsync(userId.Trim());
        if (user is null)
            throw new BadRequestException("User not found.");

        user.MustChangePassword = false;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Password change failed: {errors}");
        }

        await _userManager.UpdateAsync(user);
    }

    public async Task SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Email must be provided.");

        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null)
            throw new BadRequestException("User not found.");

        if (string.IsNullOrWhiteSpace(user.Email))
            throw new BadRequestException("User email is missing.");

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
            .To(user.Email)
            .Subject(template.GetSubject())
            .PlainTextBody(template.GetPlainTextBody(data))
            .HtmlBody(template.GetHtmlBody(data))
            .Build();

        await _emailService.SendAsync(message, cancellationToken);
    }

    public async Task ResetPasswordAsync(string userId, string encodedToken, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new BadRequestException("UserId must be provided.");

        if (string.IsNullOrWhiteSpace(encodedToken))
            throw new BadRequestException("Token must be provided.");

        var user = await _userManager.FindByIdAsync(userId.Trim());
        if (user is null)
            throw new BadRequestException("User not found.");

        user.MustChangePassword = false;

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken));
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Password reset failed: {errors}");
        }

        await _userManager.UpdateAsync(user);
    }

    private async Task<ApplicationUserEntity> ValidateCredentialsAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Email must be provided.");

        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null)
            throw new UnauthenticatedException("Invalid email.");

        var validPassword = await _userManager.CheckPasswordAsync(user, password);
        return !validPassword ? throw new UnauthenticatedException("Invalid password.") : user;
    }

    private async Task<IList<string>> GetRolesAsync(ApplicationUserEntity user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return roles.Count == 0 ? throw new NoRolesException($"User '{user.Email}' has no roles assigned.") : roles;
    }

    private async Task CreateUserAsync(ApplicationUserEntity user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded) return;

        var errors = string.Join(" ", result.Errors.Select(e => e.Description));
        throw new BadRequestException($"User creation failed: {errors}");
    }

    private async Task AssignRoleAsync(ApplicationUserEntity user, UserRole role)
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

    private async Task SendConfirmationEmailAsync(ApplicationUserEntity user, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
            throw new BadRequestException("User email is missing.");

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
            .To(user.Email)
            .Subject(template.GetSubject())
            .PlainTextBody(template.GetPlainTextBody(data))
            .HtmlBody(template.GetHtmlBody(data))
            .Build();

        await _emailService.SendAsync(message, cancellationToken);
    }

    private static ApplicationUserEntity CreateApplicationUserEntity(
        string fullName,
        Department department,
        DateOnly dateOfBirth,
        string tenantId,
        string email,
        string phoneNumber)
    {
        var user = new ApplicationUserEntity
        {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            UserName = email.Trim(),
            Department = department,
            TenantId = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId.Trim(),
            DateOfBirth = dateOfBirth,
            PhoneNumber = phoneNumber?.Trim()
        };

        user.ContactDetails = new ContactDetailsOwned
        {
            Email = email.Trim(),
            PhoneNumber = phoneNumber?.Trim() ?? "",
            Socials = new SocialLinksOwned()
        };

        return user;
    }
}