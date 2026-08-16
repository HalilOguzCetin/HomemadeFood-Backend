using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Infrastructure
{
    public sealed class VerificationCodeService :
        IVerificationCodeService
    {
        private readonly byte[] _hashKey;

        public VerificationCodeService(
            byte[] hashKey)
        {
            if (hashKey == null ||
                hashKey.Length < 32)
            {
                throw new ArgumentException(
                    "Verification hash key en az 32 byte olmalıdır.",
                    nameof(hashKey));
            }

            _hashKey =
                hashKey.ToArray();
        }

        public string GenerateSixDigitCode()
        {
            var value =
                RandomNumberGenerator.GetInt32(
                    0,
                    1_000_000);

            return value.ToString(
                "D6",
                CultureInfo.InvariantCulture);
        }

        public string HashTarget(
            string target)
        {
            if (string.IsNullOrWhiteSpace(
                    target))
            {
                throw new ArgumentException(
                    "Doğrulama hedefi boş olamaz.",
                    nameof(target));
            }

            var normalizedTarget =
                target
                    .Trim()
                    .ToLowerInvariant();

            return ComputeHmac(
                normalizedTarget);
        }

        public string HashSecret(
            string secret)
        {
            if (string.IsNullOrWhiteSpace(
                    secret))
            {
                throw new ArgumentException(
                    "Doğrulama kodu boş olamaz.",
                    nameof(secret));
            }

            return ComputeHmac(
                secret);
        }

        public bool VerifySecret(
            string secret,
            string expectedHash)
        {
            if (string.IsNullOrWhiteSpace(
                    secret) ||
                string.IsNullOrWhiteSpace(
                    expectedHash))
            {
                return false;
            }

            byte[] expectedBytes;

            try
            {
                expectedBytes =
                    Convert.FromBase64String(
                        expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }

            var actualHash =
                HashSecret(secret);

            var actualBytes =
                Convert.FromBase64String(
                    actualHash);

            return CryptographicOperations
                .FixedTimeEquals(
                    actualBytes,
                    expectedBytes);
        }

        public bool VerifyTarget(
            string target,
            string expectedHash)
        {
            if (
                string.IsNullOrWhiteSpace(target) ||
                string.IsNullOrWhiteSpace(expectedHash)
            )
            {
                return false;
            }

            byte[] expectedBytes;

            try
            {
                expectedBytes =
                    Convert.FromBase64String(
                        expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }

            var actualHash =
                HashTarget(target);

            var actualBytes =
                Convert.FromBase64String(
                    actualHash);

            return CryptographicOperations
                .FixedTimeEquals(
                    actualBytes,
                    expectedBytes);
        }

        private string ComputeHmac(
            string value)
        {
            using var hmac =
                new HMACSHA256(
                    _hashKey);

            var bytes =
                Encoding.UTF8.GetBytes(
                    value);

            var hash =
                hmac.ComputeHash(
                    bytes);

            return Convert.ToBase64String(
                hash);
        }
    }
}