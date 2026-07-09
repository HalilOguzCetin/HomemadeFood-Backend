using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}