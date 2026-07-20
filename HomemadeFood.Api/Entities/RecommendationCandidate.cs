namespace HomemadeFood.Api.Entities
{
    public class RecommendationCandidate
    {
        public int Id { get; set; }

        public int RecommendationSearchId { get; set; }

        public RecommendationSearch
            RecommendationSearch
        { get; set; } = null!;

        // Arama sonucunda gösterilen yemek ve üretici.
        public int FoodId { get; set; }

        public string FoodName { get; set; } =
            string.Empty;

        public decimal Price { get; set; }

        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; } =
            string.Empty;

        // Kullanıcıya gösterildiği sıra:
        // 1, 2 veya 3.
        public int Rank { get; set; }

        public double DistanceKm { get; set; }

        public decimal AverageRating { get; set; }

        public int ReviewCount { get; set; }

        public int PreparationTimeMinutes { get; set; }

        public int RemainingCapacity { get; set; }

        public double RatingScore { get; set; }

        public double DistanceScore { get; set; }

        public double PreparationScore { get; set; }

        public double TotalScore { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}