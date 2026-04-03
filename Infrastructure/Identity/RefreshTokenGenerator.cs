using Application.Interfaces;
using System.Security.Cryptography;

namespace Infrastructure.Identity;

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}