namespace HomemadeFood.Api.DTOs.Admin
{
    public class AdminOrderListItemResponse
    {
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerFullName { get; set; } =
            string.Empty;

        public string CustomerEmail { get; set; } =
            string.Empty;

        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; } =
            string.Empty;

        public string Status { get; set; } =
            string.Empty;

        public string PaymentMethod { get; set; } =
            string.Empty;

        public decimal TotalPrice { get; set; }

        public int ItemCount { get; set; }

        public int TotalQuantity { get; set; }

        public int? RecommendationSearchId { get; set; }

        public decimal SuitabilityScore { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime StatusUpdatedAt { get; set; }
    }
}