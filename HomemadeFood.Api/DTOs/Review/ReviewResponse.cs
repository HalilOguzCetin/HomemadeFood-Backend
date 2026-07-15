namespace HomemadeFood.Api.DTOs.Review
{
    public class ReviewResponse
    {
        public int ReviewId { get; set; }

        public int OrderId { get; set; }

        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; } = string.Empty;

        public string CustomerFullName { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}