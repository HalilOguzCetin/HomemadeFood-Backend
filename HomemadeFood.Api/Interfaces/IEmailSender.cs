namespace HomemadeFood.Api.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailVerificationCodeAsync(
            string email,
            string code);
    }
}