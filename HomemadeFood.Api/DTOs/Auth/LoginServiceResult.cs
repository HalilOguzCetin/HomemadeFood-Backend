namespace HomemadeFood.Api.DTOs.Auth
{
    public enum LoginResultStatus
    {
        Success,
        InvalidCredentials,
        EmailNotVerified
    }

    public sealed class LoginServiceResult
    {
        public LoginResultStatus Status { get; init; }

        public LoginResponse? Response { get; init; }

        public string? Email { get; init; }

        public static LoginServiceResult Success(
            LoginResponse response)
        {
            return new LoginServiceResult
            {
                Status =
                    LoginResultStatus.Success,

                Response =
                    response
            };
        }

        public static LoginServiceResult
            InvalidCredentials()
        {
            return new LoginServiceResult
            {
                Status =
                    LoginResultStatus
                        .InvalidCredentials
            };
        }

        public static LoginServiceResult
            EmailNotVerified(
                string email)
        {
            return new LoginServiceResult
            {
                Status =
                    LoginResultStatus
                        .EmailNotVerified,

                Email =
                    email
            };
        }
    }
}