using HomemadeFood.Api.Constants;

namespace HomemadeFood.Api.Entities
{
    public class Food
    {
        public int Id { get; set; }

        public int ProducerProfileId { get; set; }
        public ProducerProfile ProducerProfile { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /*
         * Üreticinin yemek için girdiği gerçek içerik listesi.
         * Alerjen bilgisi bu serbest metinden türetilmez;
         * FoodAllergens ile ayrıca yapılandırılmış olarak tutulur.
         */
        public string Ingredients { get; set; } = string.Empty;

        public decimal Price { get; set; }

        /*
         * Fiyatın ve miktarın hangi üretim birimine ait olduğunu
         * belirtir: Portion, Tray, Piece, Kilogram, Liter,
         * Package veya Other.
         */
        public string UnitType { get; set; } =
            FoodUnitTypes.Portion;

        /*
         * Bu yemeğin bir takvim gününde üretilebilecek toplam
         * miktarıdır. Kalan kapasite daha sonra siparişlerden
         * türetilir; burada ayrıca mutable RemainingCapacity
         * tutulmaz.
         */
        public int DailyCapacity { get; set; } = 1;

        public int PreparationTimeMinutes { get; set; }

        /*
         * Siparişin üretici tarafından hazırlanabilmesi için
         * gereken minimum ön bildirim süresidir.
         * 0 = ayrıca minimum ön süre gerekmiyor.
         */
        public int MinimumOrderLeadTimeMinutes { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<FoodAllergen> Allergens { get; set; } =
            new List<FoodAllergen>();

        public ICollection<OrderItem> OrderItems { get; set; } =
            new List<OrderItem>();

        public ICollection<Favorite> Favorites { get; set; } =
            new List<Favorite>();

        public ICollection<CartItem> CartItems { get; set; } =
            new List<CartItem>();
    }
}
