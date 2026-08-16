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

        Task<AuthProfileResponse?> UpdateProfileAsync(
            int userId,
            UpdateAuthProfileRequest request);

        Task<bool> RequestPhoneVerificationAsync(
            int userId,
            RequestPhoneVerificationRequest request);

        Task<AuthProfileResponse?> VerifyPhoneAsync(
            int userId,
            VerifyPhoneRequest request);

        Task<bool> VerifyEmailAsync(
            VerifyEmailRequest request);

        Task ResendEmailVerificationAsync(
            ResendEmailVerificationRequest request);

        Task ForgotPasswordAsync(
            ForgotPasswordRequest request);

        Task<bool> ResetPasswordAsync(
            ResetPasswordRequest request);
    }
}