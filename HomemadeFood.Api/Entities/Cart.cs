namespace HomemadeFood.Api.Entities
{
    public class Cart
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public int ProducerProfileId { get; set; }

        public ProducerProfile ProducerProfile { get; set; }
            = null!;

        // Sepet bir öneri sonucundan oluşturulduysa
        // ilgili arama kaydının ID değeri tutulur.
        public int? RecommendationSearchId { get; set; }

        public RecommendationSearch?
            RecommendationSearch
        { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; }
            = DateTime.UtcNow;

        public ICollection<CartItem> Items { get; set; }
            = new List<CartItem>();
    }
}