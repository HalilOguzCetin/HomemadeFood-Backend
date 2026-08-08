namespace HomemadeFood.Api.Interfaces
{
    public interface IVerificationCodeService
    {
        string GenerateSixDigitCode();

        string HashTarget(
            string target);

        string HashSecret(
            string secret);

        bool VerifySecret(
            string secret,
            string expectedHash);
    }
}