namespace HomemadeFood.Api.Interfaces
{
    public interface IPhoneNumberNormalizer
    {
        bool TryNormalizeTurkishMobile(
            string? phone,
            out string normalizedPhone);
    }
}