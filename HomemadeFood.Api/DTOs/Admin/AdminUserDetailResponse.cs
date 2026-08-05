namespace HomemadeFood.Api.DTOs.Admin
{
    public class AdminUserDetailResponse
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

        public int AddressCount { get; set; }

        public int OrderCount { get; set; }

        public int ReviewCount { get; set; }

        public int FavoriteCount { get; set; }

        public int? ProducerProfileId { get; set; }

        public string? BusinessName { get; set; }

        public string? ProducerVerificationStatus
        {
            get;
            set;
        }

        public bool? IsProducerApproved { get; set; }

        public bool? IsProducerAvailable { get; set; }

        public int? DailyCapacity { get; set; }

        public int? RemainingCapacity { get; set; }
    }
}