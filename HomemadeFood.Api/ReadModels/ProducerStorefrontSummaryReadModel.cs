namespace HomemadeFood.Api.ReadModels
{
    /*
     * Repository'nin yalnızca ana sayfa vitrini için ihtiyaç
     * duyulan alanları ve aggregate değerleri döndürmesini sağlar.
     *
     * Böylece ProducerProfile + bütün Food entity'lerini belleğe
     * taşımadan MySQL/EF Core tarafında projection yapılır.
     */
    public class ProducerStorefrontSummaryReadModel
    {
        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; } =
            string.Empty;

        public string Description { get; set; } =
            string.Empty;

        public string? BusinessImageUrl { get; set; }

        public decimal Rating { get; set; }

        public string City { get; set; } =
            string.Empty;

        public string District { get; set; } =
            string.Empty;

        public int AvailableFoodCount { get; set; }

        public int AvailableCategoryCount { get; set; }

        public int MatchingFoodCount { get; set; }

        public int? MinimumPreparationTimeMinutes
        {
            get;
            set;
        }
    }
}