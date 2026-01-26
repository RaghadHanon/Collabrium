using Domain.Entities;

namespace Application.Interfaces;
public interface IJwtTokenGenerator
{
    string GenerateToken(User user, IList<string> roles);
}