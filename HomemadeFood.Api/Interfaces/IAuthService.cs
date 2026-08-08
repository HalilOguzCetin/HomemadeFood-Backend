using HomemadeFood.Api.DTOs.Auth;

namespace HomemadeFood.Api.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(
            RegisterRequest request);

        Task<LoginServiceResult> LoginAsync(
            LoginRequest request);

        Task<AuthProfileResponse?> GetProfileAsync(
            int userId);

        Task<bool> VerifyEmailAsync(
            VerifyEmailRequest request);

        Task ResendEmailVerificationAsync(
            ResendEmailVerificationRequest request);
    }
}