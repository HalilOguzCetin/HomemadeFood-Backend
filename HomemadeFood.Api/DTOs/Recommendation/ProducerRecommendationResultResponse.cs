namespace HomemadeFood.Api.DTOs.Recommendation
{
    public sealed class ProducerRecommendationResultResponse
    {
        public int RecommendationSearchId { get; set; }

        public List<ProducerRecommendationResponse>
            Recommendations
        { get; set; } = new();
    }
}