using Application.Interfaces;
using Domain.Models;
using Infrastructure.Persistence.Mappers;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.Identity;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateToken(User domainUser, IList<string> roles)
    {
        var entityUser = UserMapper.ToEntity(domainUser);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, entityUser.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("fullName", entityUser.FullName),
            new("userName", entityUser.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, entityUser.Email ?? string.Empty),
            new("lastLoginAt", entityUser.LastLoginAt?.ToString("O") ?? string.Empty),
            new("tenantId", entityUser.TenantId ?? string.Empty),
            new("mustChangePassword", entityUser.MustChangePassword.ToString()),
        };

        foreach (var role in roles)
            claims.Add(new Claim("role", role));

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}