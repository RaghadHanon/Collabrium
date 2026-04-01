using Application.Common;
using Application.Common.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Emails;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
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
    private readonly AppDbContext _dbContext;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public IdentityService(
    UserManager<ApplicationUserEntity> userManager,
    RoleManager<IdentityRole> roleManager,
    IJwtTokenGenerator jwtTokenGenerator,
    AppDbContext dbContext,
    IEmailService emailService,
    IOptions<IdentitySettings> identityOptions,
    IEmailTemplateFactory emailTemplateFactory,
    IRefreshTokenGenerator refreshTokenGenerator)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _identitySettings = identityOptions?.Value ?? throw new ArgumentNullException(nameof(identityOptions));
        _emailTemplateFactory = emailTemplateFactory ?? throw new ArgumentNullException(nameof(emailTemplateFactory));
        _refreshTokenGenerator = refreshTokenGenerator ?? throw new ArgumentNullException(nameof(refreshTokenGenerator));
    }

    public async Task<User> RegisterUserAsync(
    string fullName,
    string jobTitle,
    Department department,
    string password,
    DateOnly dateOfBirth,
    UserRole role,
    string tenantId,
    string email,
    string phoneNumber,
    CancellationToken cancellationToken = default)
    {
        var userEntity = CreateApplicationUserEntity(fullName, jobTitle, department, dateOfBirth, tenantId, email, phoneNumber);
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

    public async Task<(string AccessToken, string RefreshToken, string Id, string TenantId)> LoginAsync(
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

        var accessToken = _jwtTokenGenerator.GenerateToken(UserMapper.ToDomain(userEntity), roles);

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new TokenGenerationFailedException("AccessToken generation failed.");

        var refreshToken = _refreshTokenGenerator.GenerateToken();

        var refreshTokenEntity = new RefreshTokenEntity
        {
            Token = refreshToken,
            UserId = userEntity.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (
            accessToken,
            refreshToken,
            userEntity.Id,
            userEntity.TenantId ?? string.Empty
        );
    }

    public async Task ConfirmEmailAsync(string userId, string encodedToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new BadRequestException("UserId must be provided.");

        if (string.IsNullOrWhiteSpace(encodedToken))
            throw new BadRequestException("AccessToken must be provided.");

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

        if (string.Equals(currentPassword, newPassword, StringComparison.Ordinal))
            throw new BadRequestException("New password must be different from the current password.");

        var user = await _userManager.FindByIdAsync(userId.Trim());
        if (user is null)
            throw new BadRequestException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Password change failed: {errors}");
        }

        user.MustChangePassword = false;
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

        var resetUrl = $"{_identitySettings.PasswordResetUrlBase}?UserId={user.Id}&token={encodedToken}";

        var template = _emailTemplateFactory.GetTemplate("PasswordReset");

        var data = new Dictionary<string, string>
        {
            ["FullName"] = user.FullName,
            ["newUrl"] = resetUrl
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
            throw new BadRequestException("AccessToken must be provided.");

        if (string.IsNullOrWhiteSpace(newPassword))
            throw new BadRequestException("New password must be provided.");

        var user = await _userManager.FindByIdAsync(userId.Trim());
        if (user is null)
            throw new BadRequestException("User not found.");

        var isSameAsOld = await _userManager.CheckPasswordAsync(user, newPassword);
        if (isSameAsOld)
        {
            throw new BadRequestException("New password cannot be the same as your old password.");
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken));

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Password reset failed: {errors}");
        }

        user.MustChangePassword = false;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(" ", updateResult.Errors.Select(e => e.Description));
            throw new BadRequestException($"Failed to update user state: {errors}");
        }
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
            throw new BadRequestException($"JobTitle '{roleName}' does not exist.");

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

        var confirmationUrl = $"{_identitySettings.ConfirmationUrlBase}?UserId={user.Id}&token={encodedToken}";

        var template = _emailTemplateFactory.GetTemplate("Confirmation");

        var data = new Dictionary<string, string>
        {
            ["FullName"] = user.FullName,
            ["newUrl"] = confirmationUrl
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
        string jobTitle,
        Department department,
        DateOnly dateOfBirth,
        string tenantId,
        string email,
        string phoneNumber)
    {
        var user = new ApplicationUserEntity
        {
            FullName = fullName.Trim(),
            JobTitle = jobTitle.Trim(),
            Email = email.Trim(),
            UserName = email.Trim(),
            Department = department,
            TenantId = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId.Trim(),
            DateOfBirth = dateOfBirth,
        };

        user.ContactDetails = new ContactDetailsOwned
        {
            Email = email.Trim(),
            PhoneNumber = phoneNumber?.Trim() ?? "",
            Socials = new SocialLinksOwned()
        };

        return user;
    }

    public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(
    string refreshToken,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new BadRequestException("Refresh token must be provided.");

        var storedToken = await _dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

        if (storedToken is null)
            throw new UnauthenticatedException("Invalid refresh token.");

        if (!storedToken.IsActive)
            throw new UnauthenticatedException("Refresh token is no longer active.");

        var user = storedToken.User;

        var roles = await GetRolesAsync(user);

        var newAccessToken = _jwtTokenGenerator.GenerateToken(UserMapper.ToDomain(user), roles);

        if (string.IsNullOrWhiteSpace(newAccessToken))
            throw new TokenGenerationFailedException("Access token generation failed.");

        var newRefreshToken = _refreshTokenGenerator.GenerateToken();

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        storedToken.ReplacedByToken = newRefreshToken;

        var newRefreshTokenEntity = new RefreshTokenEntity
        {
            Token = newRefreshToken,
            UserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        };

        _dbContext.RefreshTokens.Add(newRefreshTokenEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (newAccessToken, newRefreshToken);
    }

    public async Task RevokeRefreshTokenAsync(
    string refreshToken,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new BadRequestException("Refresh token must be provided.");

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

        if (storedToken is null)
            throw new BadRequestException("Refresh token not found.");

        if (!storedToken.IsActive)
            throw new BadRequestException("Refresh token is already inactive.");

        storedToken.RevokedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}