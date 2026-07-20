namespace HomemadeFood.Api.DTOs.Recommendation
{
    public sealed class ProducerRecommendationResponse
    {
        public int FoodId { get; set; }

        public string FoodName { get; set; } =
            string.Empty;

        public string? FoodDescription { get; set; }

        public decimal Price { get; set; }

        public int PreparationTimeMinutes { get; set; }

        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; } =
            string.Empty;

        public decimal AverageRating { get; set; }

        public int ReviewCount { get; set; }

        public int RemainingCapacity { get; set; }

        public double DistanceKm { get; set; }

        public double RatingScore { get; set; }

        public double DistanceScore { get; set; }

        public double PreparationScore { get; set; }

        public double TotalScore { get; set; }

        public string Explanation { get; set; } =
            string.Empty;
    }
}