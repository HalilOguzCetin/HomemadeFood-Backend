namespace HomemadeFood.Api.Constants
{
    public static class RateLimitPolicies
    {
        public const string Login =
            "LoginRateLimit";

        public const string
            EmailVerificationResend =
                "email-verification-resend";

        public const string
            PasswordResetRequest =
                "password-reset-request";

        public const string
            PasswordResetConfirm =
                "password-reset-confirm";

        public const string
            PhoneVerificationRequest =
                "phone-verification-request";

        public const string
            PhoneVerificationConfirm =
                "phone-verification-confirm";
    }
}