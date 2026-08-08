using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Infrastructure
{
    public sealed class DevelopmentEmailSender :
        IEmailSender
    {
        private readonly ILogger<DevelopmentEmailSender>
            _logger;

        private readonly IHostEnvironment
            _environment;

        public DevelopmentEmailSender(
            ILogger<DevelopmentEmailSender> logger,
            IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public Task SendEmailVerificationCodeAsync(
            string email,
            string code)
        {
            if (!_environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "DevelopmentEmailSender yalnızca Development ortamında kullanılabilir.");
            }

            var maskedEmail =
                MaskEmail(email);

            _logger.LogWarning(
                "DEV ONLY - E-posta doğrulama kodu. Email: {Email}, Code: {Code}",
                maskedEmail,
                code);

            return Task.CompletedTask;
        }

        private static string MaskEmail(
            string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return "***";
            }

            var atIndex =
                email.IndexOf('@');

            if (atIndex <= 0)
            {
                return "***";
            }

            var localPart =
                email[..atIndex];

            var domain =
                email[atIndex..];

            var visiblePrefix =
                localPart.Length >= 2
                    ? localPart[..2]
                    : localPart[..1];

            return visiblePrefix + "***" + domain;
        }
    }
}