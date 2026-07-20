using HomemadeFood.Api.Constants;

namespace HomemadeFood.Api.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public User Customer { get; set; } = null!;

        public int ProducerProfileId { get; set; }

        public ProducerProfile ProducerProfile { get; set; }
            = null!;

        // Sipariş bir öneri sonucundan oluşturulduysa
        // ilgili öneri aramasının ID değeri tutulur.
        public int? RecommendationSearchId { get; set; }

        public RecommendationSearch?
            RecommendationSearch
        { get; set; }

        public string DeliveryAddressTitle { get; set; }
            = string.Empty;

        public string DeliveryAddress { get; set; }
            = string.Empty;

        public double DeliveryLatitude { get; set; }

        public double DeliveryLongitude { get; set; }

        public string PaymentMethod { get; set; }
            = PaymentMethods.CashOnDelivery;

        public string CustomerNote { get; set; }
            = string.Empty;

        public decimal TotalPrice { get; set; }

        public string Status { get; set; }
            = OrderStatuses.Pending;

        // Öneri motorunun sipariş oluşturulduğu
        // andaki toplam puanı.
        public decimal SuitabilityScore { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime StatusUpdatedAt { get; set; }
            = DateTime.UtcNow;

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();

        public Review? Review { get; set; }
    }
}