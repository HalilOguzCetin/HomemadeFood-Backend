namespace HomemadeFood.Api.DTOs.Admin
{
    public class AdminOrderDetailResponse
    {
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerFullName { get; set; } =
            string.Empty;

        public string CustomerEmail { get; set; } =
            string.Empty;

        public string CustomerPhone { get; set; } =
            string.Empty;

        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; } =
            string.Empty;

        public string Status { get; set; } =
            string.Empty;

        public int StatusVersion { get; set; }

        public string PaymentMethod { get; set; } =
            string.Empty;

        public decimal TotalPrice { get; set; }

        public string DeliveryAddressTitle { get; set; } =
            string.Empty;

        public string DeliveryAddress { get; set; } =
            string.Empty;

        public double DeliveryLatitude { get; set; }

        public double DeliveryLongitude { get; set; }

        public string CustomerNote { get; set; } =
            string.Empty;

        public int? RecommendationSearchId { get; set; }

        public decimal SuitabilityScore { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime StatusUpdatedAt { get; set; }

        public List<AdminOrderItemResponse> Items
        {
            get;
            set;
        } = new List<AdminOrderItemResponse>();
    }
}