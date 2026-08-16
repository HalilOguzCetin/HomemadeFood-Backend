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

        /*
         * Şifre sıfırlama isteğinde yeni bir
         * PasswordReset challenge oluşturur.
         *
         * Kullanıcı yoksa, pasifse, e-posta
         * doğrulanmamışsa veya cooldown devam
         * ediyorsa null döner.
         */
        Task<string?>
            PreparePhoneVerificationAsync(
                User user,
                string normalizedPhone);

        Task<bool>
            VerifyPhoneAsync(
                User user,
                string normalizedPhone,
                string code);

        Task<string?>
            PreparePasswordResetAsync(
                string email);

        /*
         * Kod doğruysa yeni şifreyi kaydeder ve
         * PasswordReset challenge'ını tek kullanımlık
         * olacak şekilde tüketir.
         */
        Task<bool>
            ResetPasswordAsync(
                string email,
                string code,
                string newPassword);
    }
}