namespace HomemadeFood.Api.DTOs.Admin
{
    public class PendingProducerResponse
    {
        public int ProducerProfileId { get; set; }

        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string BusinessName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public int DailyCapacity { get; set; }

        public string VerificationStatus { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}