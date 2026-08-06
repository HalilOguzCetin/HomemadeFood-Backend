namespace HomemadeFood.Api.DTOs.Auth
{
    public class AuthProfileResponse
    {
        public int UserId { get; set; }

        public string FullName { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public string Role { get; set; } =
            string.Empty;

        public bool CanUseProducerMode
        {
            get;
            set;
        }

        public int? ProducerProfileId
        {
            get;
            set;
        }

        public string? ProducerVerificationStatus
        {
            get;
            set;
        }
    }
}