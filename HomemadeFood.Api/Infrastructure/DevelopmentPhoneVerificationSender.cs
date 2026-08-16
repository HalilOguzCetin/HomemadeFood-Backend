using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Infrastructure
{
    public sealed class DevelopmentPhoneVerificationSender :
        IPhoneVerificationSender
    {
        private readonly
            ILogger<DevelopmentPhoneVerificationSender>
            _logger;

        private readonly IHostEnvironment
            _environment;

        public DevelopmentPhoneVerificationSender(
            ILogger<DevelopmentPhoneVerificationSender> logger,
            IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public Task SendPhoneVerificationCodeAsync(
            string normalizedPhone,
            string code)
        {
            EnsureDevelopment();

            _logger.LogWarning(
                "DEV ONLY - Telefon doğrulama kodu. Phone: {Phone}, Code: {Code}",
                MaskPhone(normalizedPhone),
                code);

            return Task.CompletedTask;
        }

        private void EnsureDevelopment()
        {
            if (!_environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "DevelopmentPhoneVerificationSender yalnızca Development ortamında kullanılabilir.");
            }
        }

        private static string MaskPhone(
            string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return "***";
            }

            var digits =
                new string(
                    phone
                        .Where(char.IsDigit)
                        .ToArray());

            if (digits.Length < 4)
            {
                return "***";
            }

            return "***" + digits[^4..];
        }
    }
}