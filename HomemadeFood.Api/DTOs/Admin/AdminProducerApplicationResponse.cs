namespace HomemadeFood.Api.DTOs.Admin
{
    public class AdminProducerApplicationResponse
    {
        public int ProducerProfileId { get; set; }

        public int UserId { get; set; }

        public string FullName { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public string UserRole { get; set; } =
            string.Empty;

        public string BusinessName { get; set; } =
            string.Empty;

        public string Description { get; set; } =
            string.Empty;

        public string Address { get; set; } =
            string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int DailyCapacity { get; set; }

        public int RemainingCapacity { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsApproved { get; set; }

        public string VerificationStatus { get; set; } =
            string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public int? ApprovedByAdminId { get; set; }

        public DateTime? RejectedAt { get; set; }

        public int? RejectedByAdminId { get; set; }

        public string? RejectionReason { get; set; }
    }
}