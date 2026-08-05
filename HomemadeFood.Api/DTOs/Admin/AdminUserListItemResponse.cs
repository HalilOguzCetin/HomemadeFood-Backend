namespace HomemadeFood.Api.DTOs.Admin
{
    public class AdminUserListItemResponse
    {
        public int UserId { get; set; }

        public string FullName { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public string Phone { get; set; } =
            string.Empty;

        public string Role { get; set; } =
            string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? ProducerProfileId { get; set; }

        public string? BusinessName { get; set; }

        public string? ProducerVerificationStatus
        {
            get;
            set;
        }
    }
}