using HomemadeFood.Api.DTOs.Food;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using HomemadeFood.Api.DTOs.Common;
using System.Globalization;
using System.Text;


namespace HomemadeFood.Api.Services
{
    public class FoodService : IFoodService
    {
        private readonly IFoodRepository _foodRepository;
        private readonly IProducerRepository _producerRepository;
        private readonly IFoodImageStorageService _foodImageStorageService;

        public FoodService(
            IFoodRepository foodRepository,
            IProducerRepository producerRepository,
            IFoodImageStorageService foodImageStorageService)
        {
            _foodRepository = foodRepository;
            _producerRepository = producerRepository;
            _foodImageStorageService = foodImageStorageService;
        }

        public async Task<FoodResponse?> CreateFoodAsync(
            int userId,
            CreateFoodRequest request)
        {
            var producerProfile =
                await _producerRepository
                    .GetApprovedByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return null;
            }

            var category =
                await _foodRepository
                    .GetActiveCategoryByIdAsync(request.CategoryId);

            if (category == null)
            {
                return null;
            }

            if (request.Image == null)
            {
                throw new ArgumentException(
                    "Yemek fotoğrafı zorunludur.");
            }

            string? imageUrl = null;

            try
            {
                imageUrl =
                    await _foodImageStorageService
                        .SaveAsync(request.Image);

                var food = new Food
                {
                    ProducerProfileId = producerProfile.Id,
                    ProducerProfile = producerProfile,

                    CategoryId = category.Id,
                    Category = category,

                    Name = request.Name.Trim(),
                    Description = request.Description.Trim(),
                    Price = request.Price,
                    PreparationTimeMinutes =
                        request.PreparationTimeMinutes,

                    ImageUrl = imageUrl,

                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _foodRepository.AddAsync(food);
                await _foodRepository.SaveChangesAsync();

                return MapToResponse(food);
            }
            catch
            {
                /*
                 * Dosya yazıldıktan sonra DB kaydı başarısız olursa
                 * sunucuda sahipsiz görsel bırakmıyoruz.
                 */
                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    await SafeDeleteFoodImageAsync(
                        imageUrl);
                }

                throw;
            }
        }
        public async Task<List<FoodResponse>>
    GetAvailableFoodsAsync(
        int? categoryId,
        string? search)
        {
            var foods =
                await _foodRepository
                    .GetAvailableFoodsAsync(
                        categoryId,
                        search);

            return foods
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<List<PopularFoodResponse>>
            GetPopularFoodsAsync(
                int limit)
        {
            var safeLimit =
                Math.Clamp(
                    limit,
                    1,
                    20);

            var nowUtc =
                DateTime.UtcNow;

            var fromUtc =
                nowUtc.AddDays(-30);

            var candidates =
                await _foodRepository
                    .GetPopularityCandidatesAsync(
                        fromUtc);

            if (candidates.Count == 0)
            {
                return new List<
                    PopularFoodResponse>();
            }

            var maxDeliveredOrders =
                candidates.Max(candidate =>
                    candidate
                        .DeliveredOrderCount30Days);

            var maxSoldQuantity =
                candidates.Max(candidate =>
                    candidate
                        .SoldQuantity30Days);

            var maxDistinctCustomers =
                candidates.Max(candidate =>
                    candidate
                        .DistinctCustomerCount30Days);

            var maxFavoriteCount =
                candidates.Max(candidate =>
                    candidate.FavoriteCount);

            var ranked =
                candidates
                    .Select(candidate =>
                    {
                        var deliveredOrderScore =
                            NormalizePopularityMetric(
                                candidate
                                    .DeliveredOrderCount30Days,
                                maxDeliveredOrders);

                        var soldQuantityScore =
                            NormalizePopularityMetric(
                                candidate
                                    .SoldQuantity30Days,
                                maxSoldQuantity);

                        var distinctCustomerScore =
                            NormalizePopularityMetric(
                                candidate
                                    .DistinctCustomerCount30Days,
                                maxDistinctCustomers);

                        var favoriteScore =
                            NormalizePopularityMetric(
                                candidate.FavoriteCount,
                                maxFavoriteCount);

                        var repeatCustomerRatio =
                            candidate
                                .DistinctCustomerCount30Days >
                            0
                                ? (double)
                                  candidate
                                      .RepeatCustomerCount30Days /
                                  candidate
                                      .DistinctCustomerCount30Days
                                : 0.0;

                        var freshnessScore =
                            CalculateFreshnessScore(
                                candidate.CreatedAt,
                                nowUtc);

                        /*
                         * Kilitli H5A ağırlıkları:
                         *
                         * %40 Delivered siparişlerde bulunma
                         * %20 satılan toplam porsiyon
                         * %15 farklı müşteri
                         * %10 favori ilgisi
                         * %10 tekrar sipariş oranı
                         * %5  güncellik / yeni yemek keşfi
                         */
                        var popularityScore =
                            (
                                deliveredOrderScore *
                                    0.40 +
                                soldQuantityScore *
                                    0.20 +
                                distinctCustomerScore *
                                    0.15 +
                                favoriteScore *
                                    0.10 +
                                repeatCustomerRatio *
                                    0.10 +
                                freshnessScore *
                                    0.05
                            ) *
                            100.0;

                        return new
                        {
                            Candidate =
                                candidate,

                            PopularityScore =
                                Math.Round(
                                    popularityScore,
                                    2)
                        };
                    })
                    .OrderByDescending(item =>
                        item.PopularityScore)
                    .ThenByDescending(item =>
                        item.Candidate
                            .DeliveredOrderCount30Days)
                    .ThenByDescending(item =>
                        item.Candidate
                            .SoldQuantity30Days)
                    .ThenByDescending(item =>
                        item.Candidate
                            .FavoriteCount)
                    .ThenByDescending(item =>
                        item.Candidate
                            .CreatedAt)
                    .ToList();

            /*
             * Keşif çeşitliliği:
             * tek işletme bütün carousel'i dolduramaz.
             * Aynı ProducerProfile'dan en fazla 2 yemek.
             */
            var selected =
                new List<
                    PopularFoodResponse>();

            var producerFoodCounts =
                new Dictionary<int, int>();

            foreach (var item in ranked)
            {
                producerFoodCounts
                    .TryGetValue(
                        item.Candidate
                            .ProducerProfileId,
                        out var producerFoodCount);

                if (producerFoodCount >= 2)
                {
                    continue;
                }

                selected.Add(
                    new PopularFoodResponse
                    {
                        Id =
                            item.Candidate.Id,

                        ProducerProfileId =
                            item.Candidate
                                .ProducerProfileId,

                        BusinessName =
                            item.Candidate
                                .BusinessName,

                        CategoryId =
                            item.Candidate
                                .CategoryId,

                        CategoryName =
                            item.Candidate
                                .CategoryName,

                        Name =
                            item.Candidate.Name,

                        Description =
                            item.Candidate
                                .Description,

                        Price =
                            item.Candidate.Price,

                        PreparationTimeMinutes =
                            item.Candidate
                                .PreparationTimeMinutes,

                        ImageUrl =
                            item.Candidate
                                .ImageUrl,

                        IsAvailable =
                            item.Candidate
                                .IsAvailable,

                        CreatedAt =
                            item.Candidate
                                .CreatedAt,

                        PopularityScore =
                            item.PopularityScore,

                        DeliveredOrderCount30Days =
                            item.Candidate
                                .DeliveredOrderCount30Days,

                        SoldQuantity30Days =
                            item.Candidate
                                .SoldQuantity30Days,

                        DistinctCustomerCount30Days =
                            item.Candidate
                                .DistinctCustomerCount30Days,

                        FavoriteCount =
                            item.Candidate
                                .FavoriteCount
                    });

                producerFoodCounts[
                    item.Candidate
                        .ProducerProfileId
                ] =
                    producerFoodCount + 1;

                if (
                    selected.Count >=
                    safeLimit
                )
                {
                    break;
                }
            }

            return selected;
        }
        public async Task<
           PagedResultResponse<DiscoverFoodResponse>>
           GetDiscoverFoodsAsync(
               double latitude,
               double longitude,
               string city,
               double radiusKm,
               int page,
               int pageSize,
               int? categoryId,
               string? search)
        {
            var safePage =
                Math.Max(
                    page,
                    1);

            var safePageSize =
                Math.Clamp(
                    pageSize,
                    1,
                    50);

            var nowUtc =
                DateTime.UtcNow;

            var fromUtc =
                nowUtc.AddDays(-30);

            /*
             * Ön filtre:
             * 30 km civarı için kaba koordinat kutusu.
             * Kesin mesafe daha sonra Haversine ile kontrol edilir.
             */
            var latitudeDelta =
                radiusKm / 111.0;

            var latitudeRadians =
                DegreesToRadiansForDiscover(
                    latitude);

            var longitudeScale =
                Math.Max(
                    0.01,
                    Math.Abs(
                        Math.Cos(
                            latitudeRadians)));

            var longitudeDelta =
                radiusKm /
                (
                    111.320 *
                    longitudeScale
                );

            var candidates =
                await _foodRepository
                    .GetDiscoverCandidatesAsync(
                        categoryId,
                        search,
                        latitude -
                            latitudeDelta,
                        latitude +
                            latitudeDelta,
                        longitude -
                            longitudeDelta,
                        longitude +
                            longitudeDelta,
                        fromUtc);

            var normalizedCity =
                NormalizeDiscoverLocationText(
                    city);

            var localCandidates =
                candidates
                    .Where(candidate =>
                        NormalizeDiscoverLocationText(
                            candidate
                                .ProducerCity) ==
                        normalizedCity)
                    .ToList();

            if (
                localCandidates.Count ==
                0
            )
            {
                return new PagedResultResponse<
                    DiscoverFoodResponse>
                {
                    Page =
                        safePage,

                    PageSize =
                        safePageSize,

                    TotalCount =
                        0,

                    Items =
                        new List<
                            DiscoverFoodResponse>()
                };
            }

            var maxDeliveredOrders =
                localCandidates.Max(candidate =>
                    candidate
                        .DeliveredOrderCount30Days);

            var maxSoldQuantity =
                localCandidates.Max(candidate =>
                    candidate
                        .SoldQuantity30Days);

            var maxDistinctCustomers =
                localCandidates.Max(candidate =>
                    candidate
                        .DistinctCustomerCount30Days);

            var maxFavoriteCount =
                localCandidates.Max(candidate =>
                    candidate.FavoriteCount);

            var ranked =
                localCandidates
                    .Where(candidate =>
                        double.IsFinite(
                            candidate
                                .ProducerLatitude) &&
                        double.IsFinite(
                            candidate
                                .ProducerLongitude) &&
                        candidate
                            .ProducerLatitude >=
                            -90 &&
                        candidate
                            .ProducerLatitude <=
                            90 &&
                        candidate
                            .ProducerLongitude >=
                            -180 &&
                        candidate
                            .ProducerLongitude <=
                            180)
                    .Select(candidate =>
                    {
                        var distanceKm =
                            CalculateDiscoverDistanceKm(
                                latitude,
                                longitude,
                                candidate
                                    .ProducerLatitude,
                                candidate
                                    .ProducerLongitude);

                        var deliveredOrderScore =
                            NormalizePopularityMetric(
                                candidate
                                    .DeliveredOrderCount30Days,
                                maxDeliveredOrders);

                        var soldQuantityScore =
                            NormalizePopularityMetric(
                                candidate
                                    .SoldQuantity30Days,
                                maxSoldQuantity);

                        var distinctCustomerScore =
                            NormalizePopularityMetric(
                                candidate
                                    .DistinctCustomerCount30Days,
                                maxDistinctCustomers);

                        var favoriteScore =
                            NormalizePopularityMetric(
                                candidate.FavoriteCount,
                                maxFavoriteCount);

                        var repeatCustomerRatio =
                            candidate
                                .DistinctCustomerCount30Days >
                            0
                                ? (double)
                                  candidate
                                      .RepeatCustomerCount30Days /
                                  candidate
                                      .DistinctCustomerCount30Days
                                : 0.0;

                        var freshnessScore =
                            CalculateFreshnessScore(
                                candidate.CreatedAt,
                                nowUtc);

                        /*
                         * H5 ile aynı popülerlik skoru.
                         * Keşfet'te ana sıralama MESAFE'dir;
                         * popülerlik ikinci kriterdir.
                         */
                        var popularityScore =
                            (
                                deliveredOrderScore *
                                    0.40 +
                                soldQuantityScore *
                                    0.20 +
                                distinctCustomerScore *
                                    0.15 +
                                favoriteScore *
                                    0.10 +
                                repeatCustomerRatio *
                                    0.10 +
                                freshnessScore *
                                    0.05
                            ) *
                            100.0;

                        return new
                        {
                            Candidate =
                                candidate,

                            DistanceKm =
                                distanceKm,

                            PopularityScore =
                                Math.Round(
                                    popularityScore,
                                    2)
                        };
                    })
                    .Where(item =>
                        item.DistanceKm <=
                        radiusKm)
                    .OrderBy(item =>
                        item.DistanceKm)
                    .ThenByDescending(item =>
                        item.PopularityScore)
                    .ThenByDescending(item =>
                        item.Candidate
                            .DeliveredOrderCount30Days)
                    .ThenByDescending(item =>
                        item.Candidate
                            .FavoriteCount)
                    .ThenByDescending(item =>
                        item.Candidate
                            .CreatedAt)
                    .ToList();

            var totalCount =
                ranked.Count;

            var items =
                ranked
                    .Skip(
                        (
                            safePage -
                            1
                        ) *
                        safePageSize)
                    .Take(
                        safePageSize)
                    .Select(item =>
                        new DiscoverFoodResponse
                        {
                            Id =
                                item.Candidate.Id,

                            ProducerProfileId =
                                item.Candidate
                                    .ProducerProfileId,

                            BusinessName =
                                item.Candidate
                                    .BusinessName,

                            CategoryId =
                                item.Candidate
                                    .CategoryId,

                            CategoryName =
                                item.Candidate
                                    .CategoryName,

                            Name =
                                item.Candidate.Name,

                            Description =
                                item.Candidate
                                    .Description,

                            Price =
                                item.Candidate.Price,

                            PreparationTimeMinutes =
                                item.Candidate
                                    .PreparationTimeMinutes,

                            ImageUrl =
                                item.Candidate
                                    .ImageUrl,

                            IsAvailable =
                                item.Candidate
                                    .IsAvailable,

                            CreatedAt =
                                item.Candidate
                                    .CreatedAt,

                            DistanceKm =
                                Math.Round(
                                    item.DistanceKm,
                                    2),

                            PopularityScore =
                                item.PopularityScore
                        })
                    .ToList();

            return new PagedResultResponse<
                DiscoverFoodResponse>
            {
                Items =
                    items,

                Page =
                    safePage,

                PageSize =
                    safePageSize,

                TotalCount =
                    totalCount
            };
        }

        private static double
            NormalizePopularityMetric(
                int value,
                int maximum)
        {
            if (
                value <= 0 ||
                maximum <= 0
            )
            {
                return 0.0;
            }

            return Math.Clamp(
                (double)value / maximum,
                0.0,
                1.0);
        }

        private static double
            CalculateFreshnessScore(
                DateTime createdAt,
                DateTime nowUtc)
        {
            var ageDays =
                Math.Max(
                    0.0,
                    (
                        nowUtc -
                        createdAt
                    ).TotalDays);

            if (ageDays <= 7)
            {
                return 1.0;
            }

            if (ageDays <= 14)
            {
                return 0.75;
            }

            if (ageDays <= 30)
            {
                return 0.50;
            }

            if (ageDays <= 60)
            {
                return 0.25;
            }

            return 0.0;
        }

        public async Task<FoodResponse?>
    GetAvailableFoodByIdAsync(int foodId)
        {
            var food =
                await _foodRepository
                    .GetAvailableFoodByIdAsync(foodId);

            if (food == null)
            {
                return null;
            }

            return MapToResponse(food);
        }

        public async Task<List<FoodResponse>>
            GetMyFoodsAsync(int userId)
        {
            var producerProfile =
                await _producerRepository
                    .GetApprovedByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return new List<FoodResponse>();
            }

            var foods =
                await _foodRepository
                    .GetByProducerProfileIdAsync(
                        producerProfile.Id);

            return foods
                .Select(MapToResponse)
                .ToList();
        }
        public async Task<FoodResponse?> GetMyFoodByIdAsync(
    int userId,
    int foodId)
        {
            var producerProfile =
                await _producerRepository
                    .GetApprovedByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return null;
            }

            var food =
                await _foodRepository
                    .GetByIdAndProducerProfileIdAsync(
                        foodId,
                        producerProfile.Id);

            if (food == null)
            {
                return null;
            }

            return MapToResponse(food);
        }

        public async Task<FoodResponse?> UpdateFoodAsync(
            int userId,
            int foodId,
            UpdateFoodRequest request)
        {
            var producerProfile =
                await _producerRepository
                    .GetApprovedByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return null;
            }

            var food =
                await _foodRepository
                    .GetByIdAndProducerProfileIdAsync(
                        foodId,
                        producerProfile.Id);

            if (food == null)
            {
                return null;
            }

            var category =
                await _foodRepository
                    .GetActiveCategoryByIdAsync(
                        request.CategoryId);

            if (category == null)
            {
                return null;
            }

            var previousImageUrl =
                food.ImageUrl;

            string? replacementImageUrl =
                null;

            if (
                request.Image != null &&
                request.Image.Length > 0
            )
            {
                replacementImageUrl =
                    await _foodImageStorageService
                        .SaveAsync(request.Image);
            }

            food.CategoryId = category.Id;
            food.Category = category;

            food.Name = request.Name.Trim();
            food.Description = request.Description.Trim();
            food.Price = request.Price;

            food.PreparationTimeMinutes =
                request.PreparationTimeMinutes;

            /*
             * Yeni fotoğraf seçilmediyse mevcut görsel korunur.
             */
            food.ImageUrl =
                replacementImageUrl ??
                previousImageUrl;

            food.IsAvailable = request.IsAvailable;

            try
            {
                await _foodRepository
                    .SaveChangesAsync();
            }
            catch
            {
                /*
                 * DB update başarısız olduysa yeni yüklenen dosya
                 * sahipsiz bırakılmaz. Eski dosyaya dokunulmaz.
                 */
                await SafeDeleteFoodImageAsync(
                    replacementImageUrl);

                throw;
            }

            if (
                replacementImageUrl != null &&
                !string.Equals(
                    previousImageUrl,
                    replacementImageUrl,
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                /*
                 * DB artık yeni görsele işaret ediyor.
                 * Eski dosya temizliği başarısız olsa bile başarılı
                 * DB update'i geri çevirmiyoruz.
                 */
                await SafeDeleteFoodImageAsync(
                    previousImageUrl);
            }

            return MapToResponse(food);
        }

        public async Task<bool> DeleteFoodAsync(
            int userId,
            int foodId)
        {
            var producerProfile =
                await _producerRepository
                    .GetApprovedByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return false;
            }

            var food =
                await _foodRepository
                    .GetByIdAndProducerProfileIdAsync(
                        foodId,
                        producerProfile.Id);

            if (food == null)
            {
                return false;
            }

            food.IsAvailable = false;

            await _foodRepository.SaveChangesAsync();

            return true;
        }

        private async Task SafeDeleteFoodImageAsync(
            string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            try
            {
                await _foodImageStorageService
                    .DeleteAsync(imageUrl);
            }
            catch
            {
                /*
                 * Storage cleanup ikincil işlemdir.
                 * Başarılı DB kaydını veya asıl hatayı maskelemez.
                 */
            }
        }

        private static FoodResponse MapToResponse(Food food)
        {
            return new FoodResponse
            {
                Id = food.Id,
                ProducerProfileId =
                    food.ProducerProfileId,

                BusinessName =
                    food.ProducerProfile.BusinessName,

                CategoryId = food.CategoryId,
                CategoryName = food.Category.Name,

                Name = food.Name,
                Description = food.Description,
                Price = food.Price,

                PreparationTimeMinutes =
                    food.PreparationTimeMinutes,

                ImageUrl = food.ImageUrl,
                IsAvailable = food.IsAvailable,
                CreatedAt = food.CreatedAt
            };
        }
        private static double
           CalculateDiscoverDistanceKm(
               double latitude1,
               double longitude1,
               double latitude2,
               double longitude2)
        {
            const double earthRadiusKm =
                6371.0088;

            var latitudeDifference =
                DegreesToRadiansForDiscover(
                    latitude2 -
                    latitude1);

            var longitudeDifference =
                DegreesToRadiansForDiscover(
                    longitude2 -
                    longitude1);

            var latitude1Radians =
                DegreesToRadiansForDiscover(
                    latitude1);

            var latitude2Radians =
                DegreesToRadiansForDiscover(
                    latitude2);

            var haversine =
                Math.Pow(
                    Math.Sin(
                        latitudeDifference /
                        2.0),
                    2.0) +
                Math.Cos(
                    latitude1Radians) *
                Math.Cos(
                    latitude2Radians) *
                Math.Pow(
                    Math.Sin(
                        longitudeDifference /
                        2.0),
                    2.0);

            haversine =
                Math.Clamp(
                    haversine,
                    0.0,
                    1.0);

            return earthRadiusKm *
                (
                    2.0 *
                    Math.Asin(
                        Math.Sqrt(
                            haversine))
                );
        }

        private static double
            DegreesToRadiansForDiscover(
                double degrees)
        {
            return degrees *
                   Math.PI /
                   180.0;
        }

        private static string
            NormalizeDiscoverLocationText(
                string value)
        {
            if (
                string.IsNullOrWhiteSpace(
                    value)
            )
            {
                return string.Empty;
            }

            var normalized =
                value
                    .Trim()
                    .ToLower(
                        CultureInfo
                            .GetCultureInfo(
                                "tr-TR"))
                    .Replace(
                        'ı',
                        'i')
                    .Normalize(
                        NormalizationForm
                            .FormD);

            var builder =
                new StringBuilder();

            foreach (
                var character in
                normalized
            )
            {
                if (
                    CharUnicodeInfo
                        .GetUnicodeCategory(
                            character) !=
                    UnicodeCategory
                        .NonSpacingMark
                )
                {
                    builder.Append(
                        character);
                }
            }

            return builder
                .ToString()
                .Normalize(
                    NormalizationForm
                        .FormC);
        }

    }
}