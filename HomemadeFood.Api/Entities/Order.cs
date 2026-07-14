namespace HomemadeFood.Api.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public User Customer { get; set; } = null!;

        public int ProducerProfileId { get; set; }

        public ProducerProfile ProducerProfile { get; set; } = null!;

        // Adres bilgileri sipariş anında kopyalanır.
        // Kullanıcı kayıtlı adresini daha sonra değiştirse bile
        // eski siparişin adresi değişmez.
        public string DeliveryAddressTitle { get; set; }
            = string.Empty;

        public string DeliveryAddress { get; set; }
            = string.Empty;

        public double DeliveryLatitude { get; set; }

        public double DeliveryLongitude { get; set; }

        // CashOnDelivery veya CardOnDelivery
        public string PaymentMethod { get; set; }
            = "CashOnDelivery";

        public string CustomerNote { get; set; }
            = string.Empty;

        public decimal TotalPrice { get; set; }

        public string Status { get; set; }
            = "Pending";

        public decimal SuitabilityScore { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime StatusUpdatedAt { get; set; }
            = DateTime.UtcNow;

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();
    }
}