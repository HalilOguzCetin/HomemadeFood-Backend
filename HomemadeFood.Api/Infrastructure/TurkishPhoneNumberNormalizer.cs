using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Infrastructure
{
    public sealed class TurkishPhoneNumberNormalizer :
        IPhoneNumberNormalizer
    {
        public bool TryNormalizeTurkishMobile(
            string? phone,
            out string normalizedPhone)
        {
            normalizedPhone = string.Empty;

            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }

            var digits =
                new string(
                    phone
                        .Where(char.IsDigit)
                        .ToArray());

            string nationalNumber;

            if (
                digits.Length == 14 &&
                digits.StartsWith(
                    "0090",
                    StringComparison.Ordinal)
            )
            {
                nationalNumber = digits[4..];
            }
            else if (
                digits.Length == 12 &&
                digits.StartsWith(
                    "90",
                    StringComparison.Ordinal)
            )
            {
                nationalNumber = digits[2..];
            }
            else if (
                digits.Length == 11 &&
                digits.StartsWith(
                    "0",
                    StringComparison.Ordinal)
            )
            {
                nationalNumber = digits[1..];
            }
            else if (digits.Length == 10)
            {
                nationalNumber = digits;
            }
            else
            {
                return false;
            }

            if (
                nationalNumber.Length != 10 ||
                nationalNumber[0] != '5' ||
                !nationalNumber.All(char.IsDigit)
            )
            {
                return false;
            }

            normalizedPhone =
                "+90" + nationalNumber;

            return true;
        }
    }
}