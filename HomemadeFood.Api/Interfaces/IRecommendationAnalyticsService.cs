using HomemadeFood.Api.DTOs.Recommendation;

namespace HomemadeFood.Api.Interfaces
{
    public interface IRecommendationAnalyticsService
    {
        Task<RecommendationPerformanceResponse>
            GetPerformanceSummaryAsync();
    }
}