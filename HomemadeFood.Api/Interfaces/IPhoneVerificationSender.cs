namespace HomemadeFood.Api.Interfaces
{
    public interface IPhoneVerificationSender
    {
        Task SendPhoneVerificationCodeAsync(
            string normalizedPhone,
            string code);
    }
}