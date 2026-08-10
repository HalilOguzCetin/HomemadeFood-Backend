namespace HomemadeFood.Api.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailVerificationCodeAsync(
            string email,
            string code);

        Task SendPasswordResetCodeAsync(
            string email,
            string code);
    }
}