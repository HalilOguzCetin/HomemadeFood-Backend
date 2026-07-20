namespace HomemadeFood.Api.DTOs.Recommendation
{
    public sealed class ProducerRecommendationSelectionResponse
    {
        public int RecommendationSearchId { get; set; }

        public int FoodId { get; set; }

        public string FoodName { get; set; } =
            string.Empty;

        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; } =
            string.Empty;

        public int Rank { get; set; }

        public double TotalScore { get; set; }

        public DateTime SelectedAtUtc { get; set; }
    }
}