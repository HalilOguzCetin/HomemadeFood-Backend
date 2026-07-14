namespace HomemadeFood.Api.DTOs.Order
{
    public class OrderResponse
    {
        public int OrderId { get; set; }

        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; }
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

        public decimal TotalPrice { get; set; }

        public string Status { get; set; }
            = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime StatusUpdatedAt { get; set; }

        public List<OrderItemResponse> Items { get; set; }
            = new List<OrderItemResponse>();
    }
}