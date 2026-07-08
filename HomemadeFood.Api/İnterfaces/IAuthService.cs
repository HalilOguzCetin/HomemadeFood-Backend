using HomemadeFood.Api.DTOs.Auth;

namespace HomemadeFood.Api.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequest request);

        Task<LoginResponse?> LoginAsync(LoginRequest request);
    }
}