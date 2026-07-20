using HomemadeFood.Api.Constants;
using HomemadeFood.Api.Data;
using HomemadeFood.Api.DTOs.Recommendation;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Helpers;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Services
{
    public sealed class ProducerRecommendationService
        : IProducerRecommendationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IAppClock _appClock;

        public ProducerRecommendationService(
            AppDbContext dbContext,
            IAppClock appClock)
        {
            _dbContext = dbContext;
            _appClock = appClock;
        }

        public async Task<ProducerRecommendationResultResponse?>
            GetRecommendationsAsync(
                int customerUserId,
                ProducerRecommendationRequest request)
        {
            var address =
                await _dbContext.Addresses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == request.AddressId &&
                            x.UserId == customerUserId);

            if (address == null)
            {
                return null;
            }

            var searchText =
                request.SearchText.Trim();

            var today =
                _appClock.TurkeyToday;

            var candidates =
                await _dbContext.Foods
                    .AsNoTracking()
                    .Where(food =>
                        food.IsAvailable &&

                        food.Name.Contains(searchText) &&

                        food.ProducerProfile.IsApproved &&

                        food.ProducerProfile
                            .VerificationStatus ==
                        ProducerVerificationStatuses.Approved &&

                        food.ProducerProfile.IsAvailable &&

                        food.ProducerProfile.User.IsActive)
                    .Select(food => new
                    {
                        FoodId = food.Id,

                        FoodName = food.Name,

                        FoodDescription =
                            food.Description,

                        food.Price,

                        food.PreparationTimeMinutes,

                        ProducerProfileId =
                            food.ProducerProfileId,

                        BusinessName =
                            food.ProducerProfile
                                .BusinessName,

                        AverageRating =
                            food.ProducerProfile.Rating,

                        ReviewCount =
                            food.ProducerProfile
                                .Reviews.Count,

                        DailyCapacity =
                            food.ProducerProfile
                                .DailyCapacity,

                        RemainingCapacity =
                            food.ProducerProfile
                                .RemainingCapacity,

                        CapacityDate =
                            food.ProducerProfile
                                .CapacityDate,

                        ProducerLatitude =
                            food.ProducerProfile
                                .Latitude,

                        ProducerLongitude =
                            food.ProducerProfile
                                .Longitude
                    })
                    .ToListAsync();

            var recommendations =
                new List<ProducerRecommendationResponse>();

            foreach (var candidate in candidates)
            {
                var currentCapacity =
                    candidate.CapacityDate == today
                        ? candidate.RemainingCapacity
                        : candidate.DailyCapacity;

                var hasEnoughCapacity =
                    request.Quantity.HasValue
                        ? currentCapacity >=
                          request.Quantity.Value
                        : currentCapacity > 0;

                if (!hasEnoughCapacity)
                {
                    continue;
                }

                var distanceKm =
                    DistanceCalculator
                        .CalculateDistanceKm(
                            address.Latitude,
                            address.Longitude,
                            candidate.ProducerLatitude,
                            candidate.ProducerLongitude);

                var ratingScore =
                    RecommendationScoreCalculator
                        .CalculateRatingScore(
                            candidate.AverageRating,
                            candidate.ReviewCount);

                var distanceScore =
                    RecommendationScoreCalculator
                        .CalculateDistanceScore(
                            distanceKm);

                var preparationScore =
                    RecommendationScoreCalculator
                        .CalculatePreparationScore(
                            candidate
                                .PreparationTimeMinutes);

                var totalScore =
                    RecommendationScoreCalculator
                        .CalculateTotalScore(
                            ratingScore,
                            distanceScore,
                            preparationScore);

                var explanation =
                    RecommendationScoreCalculator
                        .CreateExplanation(
                            distanceKm,
                            candidate.AverageRating,
                            candidate.ReviewCount,
                            candidate
                                .PreparationTimeMinutes);

                recommendations.Add(
                    new ProducerRecommendationResponse
                    {
                        FoodId =
                            candidate.FoodId,

                        FoodName =
                            candidate.FoodName,

                        FoodDescription =
                            candidate.FoodDescription,

                        Price =
                            candidate.Price,

                        PreparationTimeMinutes =
                            candidate
                                .PreparationTimeMinutes,

                        ProducerProfileId =
                            candidate
                                .ProducerProfileId,

                        BusinessName =
                            candidate.BusinessName,

                        AverageRating =
                            candidate.AverageRating,

                        ReviewCount =
                            candidate.ReviewCount,

                        RemainingCapacity =
                            currentCapacity,

                        DistanceKm =
                            distanceKm,

                        RatingScore =
                            ratingScore,

                        DistanceScore =
                            distanceScore,

                        PreparationScore =
                            preparationScore,

                        TotalScore =
                            totalScore,

                        Explanation =
                            explanation
                    });
            }

            var topRecommendations =
                recommendations
                    .GroupBy(x =>
                        x.ProducerProfileId)
                    .Select(group =>
                        group
                            .OrderByDescending(x =>
                                x.TotalScore)
                            .ThenBy(x =>
                                x.Price)
                            .First())
                    .OrderByDescending(x =>
                        x.TotalScore)
                    .ThenBy(x =>
                        x.DistanceKm)
                    .ThenBy(x =>
                        x.PreparationTimeMinutes)
                    .Take(3)
                    .ToList();

            var createdAtUtc =
                _appClock.UtcNow;

            var searchRecord =
                new RecommendationSearch
                {
                    CustomerUserId =
                        customerUserId,

                    AddressId =
                        address.Id,

                    CustomerLatitude =
                        address.Latitude,

                    CustomerLongitude =
                        address.Longitude,

                    SearchText =
                        searchText,

                    RequestedQuantity =
                        request.Quantity,

                    CreatedAtUtc =
                        createdAtUtc
                };

            for (var index = 0;
                 index < topRecommendations.Count;
                 index++)
            {
                var recommendation =
                    topRecommendations[index];

                searchRecord.Candidates.Add(
                    new RecommendationCandidate
                    {
                        FoodId =
                            recommendation.FoodId,

                        FoodName =
                            recommendation.FoodName,

                        Price =
                            recommendation.Price,

                        ProducerProfileId =
                            recommendation
                                .ProducerProfileId,

                        BusinessName =
                            recommendation
                                .BusinessName,

                        Rank =
                            index + 1,

                        DistanceKm =
                            recommendation.DistanceKm,

                        AverageRating =
                            recommendation.AverageRating,

                        ReviewCount =
                            recommendation.ReviewCount,

                        PreparationTimeMinutes =
                            recommendation
                                .PreparationTimeMinutes,

                        RemainingCapacity =
                            recommendation
                                .RemainingCapacity,

                        RatingScore =
                            recommendation.RatingScore,

                        DistanceScore =
                            recommendation.DistanceScore,

                        PreparationScore =
                            recommendation
                                .PreparationScore,

                        TotalScore =
                            recommendation.TotalScore,

                        CreatedAtUtc =
                            createdAtUtc
                    });
            }

            _dbContext.RecommendationSearches.Add(
                searchRecord);

            await _dbContext.SaveChangesAsync();

            return new ProducerRecommendationResultResponse
            {
                RecommendationSearchId =
                    searchRecord.Id,

                Recommendations =
                    topRecommendations
            };
        }
        public async Task<RecommendationSelectionResult>
    SelectRecommendationAsync(
        int customerUserId,
        ProducerRecommendationSelectionRequest request)
        {
            var searchRecord =
                await _dbContext.RecommendationSearches
                    .Include(x => x.Candidates)
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                            request.RecommendationSearchId &&

                            x.CustomerUserId ==
                            customerUserId);

            if (searchRecord == null)
            {
                return RecommendationSelectionResult.Fail(
                    ApiResponseCodes
                        .RecommendationSearchNotFound,
                    "Öneri araması bulunamadı veya giriş yapan müşteriye ait değil.");
            }

            var selectedCandidate =
                searchRecord.Candidates
                    .FirstOrDefault(
                        x => x.FoodId == request.FoodId);

            if (selectedCandidate == null)
            {
                return RecommendationSelectionResult.Fail(
                    ApiResponseCodes
                        .RecommendationCandidateNotFound,
                    "Seçilen yemek bu öneri aramasındaki adaylar arasında bulunamadı.");
            }

            if (searchRecord.SelectedFoodId.HasValue)
            {
                var sameSelection =
                    searchRecord.SelectedFoodId ==
                        selectedCandidate.FoodId &&

                    searchRecord.SelectedProducerProfileId ==
                        selectedCandidate.ProducerProfileId;

                if (!sameSelection)
                {
                    return RecommendationSelectionResult.Fail(
                        ApiResponseCodes
                            .RecommendationAlreadySelected,
                        "Bu öneri araması için daha önce farklı bir seçim yapılmış.");
                }

                return RecommendationSelectionResult.Succeed(
                    CreateSelectionResponse(
                        searchRecord,
                        selectedCandidate),
                    "Bu öneri daha önce seçilmiş.");
            }

            searchRecord.SelectedFoodId =
                selectedCandidate.FoodId;

            searchRecord.SelectedProducerProfileId =
                selectedCandidate.ProducerProfileId;

            searchRecord.SelectedAtUtc =
                _appClock.UtcNow;

            await _dbContext.SaveChangesAsync();

            return RecommendationSelectionResult.Succeed(
                CreateSelectionResponse(
                    searchRecord,
                    selectedCandidate),
                "Öneri seçimi başarıyla kaydedildi.");
        }

        private static ProducerRecommendationSelectionResponse
            CreateSelectionResponse(
                RecommendationSearch searchRecord,
                RecommendationCandidate candidate)
        {
            return new ProducerRecommendationSelectionResponse
            {
                RecommendationSearchId =
                    searchRecord.Id,

                FoodId =
                    candidate.FoodId,

                FoodName =
                    candidate.FoodName,

                ProducerProfileId =
                    candidate.ProducerProfileId,

                BusinessName =
                    candidate.BusinessName,

                Rank =
                    candidate.Rank,

                TotalScore =
                    candidate.TotalScore,

                SelectedAtUtc =
                    searchRecord.SelectedAtUtc
                    ?? DateTime.UtcNow
            };
        }
    }
}