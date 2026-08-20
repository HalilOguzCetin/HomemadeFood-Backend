namespace HomemadeFood.Api.ReadModels
{
    /*
     * H4B:
     * Popüler işletme skoru için repository'den service'e
     * taşınan ham metrikler.
     *
     * Skor repository içinde hesaplanmaz. Böylece veri erişimi
     * ile iş kuralı birbirinden ayrılır.
     */
    public class ProducerPopularityCandidateReadModel
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

        public int? MinimumPreparationTimeMinutes
        {
            get;
            set;
        }

        /*
         * Son 30 günlük davranış sinyalleri.
         */
        public int DeliveredOrderCount30Days
        {
            get;
            set;
        }

        public int DistinctCustomerCount30Days
        {
            get;
            set;
        }

        public int RepeatCustomerCount30Days
        {
            get;
            set;
        }

        /*
         * Favorilerde CreatedAt alanı bulunmadığı için
         * bu sinyal all-time toplamdır.
         */
        public int FavoriteCount { get; set; }

        /*
         * Bayesian rating güvenilirliği için toplam yorum sayısı.
         */
        public int ReviewCount { get; set; }
    }
}