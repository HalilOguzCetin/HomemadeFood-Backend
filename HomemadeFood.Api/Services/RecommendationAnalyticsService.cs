using HomemadeFood.Api.Constants;
using HomemadeFood.Api.Data;
using HomemadeFood.Api.DTOs.Recommendation;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Services
{
    public sealed class RecommendationAnalyticsService
        : IRecommendationAnalyticsService
    {
        private readonly AppDbContext _dbContext;

        public RecommendationAnalyticsService(
            AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RecommendationPerformanceResponse>
            GetPerformanceSummaryAsync()
        {
            var totalSearches =
                await _dbContext
                    .RecommendationSearches
                    .AsNoTracking()
                    .CountAsync();

            var searchesWithRecommendations =
                await _dbContext
                    .RecommendationSearches
                    .AsNoTracking()
                    .CountAsync(
                        search =>
                            search.Candidates.Any());

            var searchesWithoutRecommendations =
                totalSearches -
                searchesWithRecommendations;

            var totalCandidatesShown =
                await _dbContext
                    .RecommendationCandidates
                    .AsNoTracking()
                    .CountAsync();

            var selectedSearches =
                await _dbContext
                    .RecommendationSearches
                    .AsNoTracking()
                    .CountAsync(
                        search =>
                            search.SelectedFoodId.HasValue &&
                            search.SelectedProducerProfileId
                                .HasValue &&
                            search.SelectedAtUtc.HasValue);

            var recommendationOrdersQuery =
                _dbContext.Orders
                    .AsNoTracking()
                    .Where(
                        order =>
                            order.RecommendationSearchId
                                .HasValue);

            var recommendationOrders =
                await recommendationOrdersQuery
                    .CountAsync();

            var deliveredOrders =
                await recommendationOrdersQuery
                    .CountAsync(
                        order =>
                            order.Status ==
                            OrderStatuses.Delivered);

            var cancelledOrders =
                await recommendationOrdersQuery
                    .CountAsync(
                        order =>
                            order.Status ==
                            OrderStatuses.Cancelled);

            var rejectedOrders =
                await recommendationOrdersQuery
                    .CountAsync(
                        order =>
                            order.Status ==
                            OrderStatuses.Rejected);

            var reviewedOrders =
                await _dbContext.Reviews
                    .AsNoTracking()
                    .CountAsync(
                        review =>
                            review.Order
                                .RecommendationSearchId
                                .HasValue);

            var suitabilityScores =
                await recommendationOrdersQuery
                    .Select(
                        order =>
                            order.SuitabilityScore)
                    .ToListAsync();

            var averageSuitabilityScore =
                suitabilityScores.Count == 0
                    ? 0
                    : Math.Round(
                        suitabilityScores.Average(),
                        2);

            var customerRatings =
                await _dbContext.Reviews
                    .AsNoTracking()
                    .Where(
                        review =>
                            review.Order
                                .RecommendationSearchId
                                .HasValue)
                    .Select(
                        review =>
                            review.Rating)
                    .ToListAsync();

            var averageCustomerRating =
                customerRatings.Count == 0
                    ? 0
                    : Math.Round(
                        customerRatings.Average(),
                        2);

            var selectedRanks =
                await (
                    from search in
                        _dbContext
                            .RecommendationSearches
                            .AsNoTracking()

                    join candidate in
                        _dbContext
                            .RecommendationCandidates
                            .AsNoTracking()

                    on new
                    {
                        RecommendationSearchId =
                            search.Id,

                        FoodId =
                            search.SelectedFoodId,

                        ProducerProfileId =
                            search
                                .SelectedProducerProfileId
                    }
                    equals new
                    {
                        RecommendationSearchId =
                            candidate
                                .RecommendationSearchId,

                        FoodId =
                            (int?)candidate.FoodId,

                        ProducerProfileId =
                            (int?)candidate
                                .ProducerProfileId
                    }

                    where
                        search.SelectedFoodId.HasValue &&
                        search
                            .SelectedProducerProfileId
                            .HasValue

                    select candidate.Rank
                )
                .ToListAsync();

            var averageSelectedRank =
                selectedRanks.Count == 0
                    ? 0
                    : Math.Round(
                        selectedRanks.Average(),
                        2);

            return new RecommendationPerformanceResponse
            {
                TotalSearches =
                    totalSearches,

                SearchesWithRecommendations =
                    searchesWithRecommendations,

                SearchesWithoutRecommendations =
                    searchesWithoutRecommendations,

                TotalCandidatesShown =
                    totalCandidatesShown,

                SelectedSearches =
                    selectedSearches,

                RecommendationOrders =
                    recommendationOrders,

                DeliveredOrders =
                    deliveredOrders,

                CancelledOrders =
                    cancelledOrders,

                RejectedOrders =
                    rejectedOrders,

                ReviewedOrders =
                    reviewedOrders,

                SearchToSelectionRate =
                    CalculateRate(
                        selectedSearches,
                        totalSearches),

                SelectionToOrderRate =
                    CalculateRate(
                        recommendationOrders,
                        selectedSearches),

                OrderDeliveryRate =
                    CalculateRate(
                        deliveredOrders,
                        recommendationOrders),

                ReviewRate =
                    CalculateRate(
                        reviewedOrders,
                        deliveredOrders),

                AverageSuitabilityScore =
                    averageSuitabilityScore,

                AverageCustomerRating =
                    averageCustomerRating,

                AverageSelectedRank =
                    averageSelectedRank
            };
        }

        private static double CalculateRate(
            int part,
            int total)
        {
            if (total <= 0)
            {
                return 0;
            }

            return Math.Round(
                part * 100.0 / total,
                2);
        }
    }
}