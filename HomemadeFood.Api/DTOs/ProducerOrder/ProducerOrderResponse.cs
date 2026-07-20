using HomemadeFood.Api.DTOs.Order;

namespace HomemadeFood.Api.DTOs.ProducerOrder
{
    public class ProducerOrderResponse
    {
        public int OrderId { get; set; }

        public int? RecommendationSearchId { get; set; }

        public decimal SuitabilityScore { get; set; }

        public string CustomerFullName { get; set; }
            = string.Empty;

        public string CustomerPhone { get; set; }
            = string.Empty;

        public string DeliveryAddressTitle { get; set; }
            = string.Empty;

        public string DeliveryAddress { get; set; }
            = string.Empty;

        public double DeliveryLatitude { get; set; }

        public double DeliveryLongitude { get; set; }

        public string PaymentMethod { get; set; }
            = string.Empty;

        public string CustomerNote { get; set; }
            = string.Empty;

        public int TotalQuantity { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; }
            = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime StatusUpdatedAt { get; set; }

        public List<OrderItemResponse> Items { get; set; }
            = new List<OrderItemResponse>();
    }
}