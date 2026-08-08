using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface IVerificationChallengeService
    {
        Task<string>
            PrepareEmailVerificationAsync(
                User user);

        Task<bool>
            VerifyEmailAsync(
                string email,
                string code);
        Task<string?>
    PrepareEmailVerificationResendAsync(
        string email);
    }
}