using HomemadeFood.Api.DTOs.Recommendation;

namespace HomemadeFood.Api.Interfaces
{
    public interface IProducerRecommendationService
    {
        Task<ProducerRecommendationResultResponse?>
            GetRecommendationsAsync(
                int customerUserId,
                ProducerRecommendationRequest request);

        Task<RecommendationSelectionResult>
            SelectRecommendationAsync(
                int customerUserId,
                ProducerRecommendationSelectionRequest request);
    }
}