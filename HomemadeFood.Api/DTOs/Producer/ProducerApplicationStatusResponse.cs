namespace HomemadeFood.Api.DTOs.Producer
{
    public sealed class ProducerApplicationStatusResponse
    {
        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; }
            = string.Empty;

        public string Description { get; set; }
            = string.Empty;

        public string Address { get; set; }
            = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int DailyCapacity { get; set; }

        public int RemainingCapacity { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsApproved { get; set; }

        public string VerificationStatus { get; set; }
            = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }

        public string? RejectionReason { get; set; }
    }
}