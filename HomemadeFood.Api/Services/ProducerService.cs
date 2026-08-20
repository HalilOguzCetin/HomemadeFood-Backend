using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Producer;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Services
{
    public class ProducerService : IProducerService
    {
        private readonly IProducerRepository _producerRepository;
        private readonly IProducerImageStorageService
            _producerImageStorageService;
        private readonly IAppClock _appClock;

        public ProducerService(
            IProducerRepository producerRepository,
            IProducerImageStorageService producerImageStorageService,
            IAppClock appClock)
        {
            _producerRepository = producerRepository;
            _producerImageStorageService =
                producerImageStorageService;
            _appClock = appClock;
        }

        public async Task<bool> ApplyAsync(
            int userId,
            ProducerApplicationRequest request)
        {
            var businessName =
                request.BusinessName?.Trim() ?? string.Empty;

            var description =
                request.Description?.Trim() ?? string.Empty;

            var address =
                request.Address?.Trim() ?? string.Empty;

            var city =
                request.City?.Trim() ?? string.Empty;

            var district =
                request.District?.Trim() ?? string.Empty;

            var neighborhood =
                request.Neighborhood?.Trim() ?? string.Empty;

            var street =
                request.Street?.Trim() ?? string.Empty;

            var buildingNo =
                request.BuildingNo?.Trim() ?? string.Empty;

            var floor =
                NormalizeOptional(request.Floor);

            var apartmentNo =
                NormalizeOptional(request.ApartmentNo);

            var addressNote =
                NormalizeOptional(request.AddressNote);

            if (!IsRequiredTextValid(
                    businessName,
                    2,
                    150) ||
                !IsRequiredTextValid(
                    description,
                    10,
                    1000) ||
                !IsRequiredTextValid(
                    address,
                    10,
                    500) ||
                !IsRequiredTextValid(
                    city,
                    1,
                    100) ||
                !IsRequiredTextValid(
                    district,
                    1,
                    100) ||
                !IsRequiredTextValid(
                    neighborhood,
                    1,
                    120) ||
                !IsRequiredTextValid(
                    street,
                    1,
                    150) ||
                !IsRequiredTextValid(
                    buildingNo,
                    1,
                    30))
            {
                return false;
            }

            if (!IsOptionalTextValid(
                    floor,
                    20) ||
                !IsOptionalTextValid(
                    apartmentNo,
                    20) ||
                !IsOptionalTextValid(
                    addressNote,
                    300))
            {
                return false;
            }

            if (
                !double.TryParse(
                    request.Latitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var latitude) ||
                latitude < -90 ||
                latitude > 90
            )
            {
                throw new ArgumentException(
                    "Enlem bilgisi geçersizdir.");
            }

            if (
                !double.TryParse(
                    request.Longitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var longitude) ||
                longitude < -180 ||
                longitude > 180
            )
            {
                throw new ArgumentException(
                    "Boylam bilgisi geçersizdir.");
            }

            if (request.DailyCapacity < 1 ||
                request.DailyCapacity > 1000)
            {
                return false;
            }

            var existingApplication =
                await _producerRepository
                    .GetByUserIdAsync(userId);

            if (
                existingApplication != null &&
                !string.Equals(
                    existingApplication
                        .VerificationStatus,
                    ProducerVerificationStatuses
                        .Rejected,
                    StringComparison.Ordinal)
            )
            {
                return false;
            }

            /*

             * Yeni başvuruda vitrin görseli zorunludur.

             * Reddedilmiş bir başvuruda mevcut görsel varsa

             * kullanıcı aynı görselle yeniden başvurabilir.

             */
            if (
                (request.BusinessImage == null ||
                 request.BusinessImage.Length <= 0) &&
                (existingApplication == null ||
                 string.IsNullOrWhiteSpace(
                     existingApplication.BusinessImageUrl))
            )
            {
                throw new ArgumentException(
                    "İşletme vitrin görseli zorunludur.");
            }

            if (existingApplication == null)
            {
                string? businessImageUrl = null;

                try
                {
                    businessImageUrl =
                        await _producerImageStorageService
                            .SaveAsync(
                                request.BusinessImage!);

                    var producerProfile =
                        new ProducerProfile
                        {
                            UserId = userId,
                            BusinessName = businessName,
                            Description = description,

                            BusinessImageUrl =
                                businessImageUrl,

                            Address = address,

                            City = city,
                            District = district,
                            Neighborhood = neighborhood,
                            Street = street,
                            BuildingNo = buildingNo,
                            Floor = floor,
                            ApartmentNo = apartmentNo,
                            AddressNote = addressNote,

                            Latitude = latitude,
                            Longitude = longitude,

                            DailyCapacity =
                                request.DailyCapacity,

                            RemainingCapacity =
                                request.DailyCapacity,

                            CapacityDate =
                                _appClock.TurkeyToday,

                            Rating = 0,

                            IsAvailable = false,
                            IsApproved = false,

                            VerificationStatus =
                                ProducerVerificationStatuses
                                    .Pending,

                            CreatedAt =
                                _appClock.UtcNow
                        };

                    await _producerRepository
                        .AddAsync(producerProfile);

                    await _producerRepository
                        .SaveChangesAsync();

                    return true;
                }
                catch
                {
                    await SafeDeleteBusinessImageAsync(
                        businessImageUrl);

                    throw;
                }
            }

            var previousBusinessImageUrl =
                existingApplication.BusinessImageUrl;

            string? replacementBusinessImageUrl =
                null;

            if (
                request.BusinessImage != null &&
                request.BusinessImage.Length > 0
            )
            {
                replacementBusinessImageUrl =
                    await _producerImageStorageService
                        .SaveAsync(
                            request.BusinessImage);
            }

            existingApplication.BusinessName =
                businessName;

            existingApplication.Description =
                description;

            existingApplication.BusinessImageUrl =
                replacementBusinessImageUrl ??
                previousBusinessImageUrl;

            existingApplication.Address =
                address;

            ApplyStructuredAddress(
                existingApplication,
                city,
                district,
                neighborhood,
                street,
                buildingNo,
                floor,
                apartmentNo,
                addressNote,
                latitude,
                longitude);

            existingApplication.DailyCapacity =
                request.DailyCapacity;

            existingApplication.RemainingCapacity =
                request.DailyCapacity;

            existingApplication.CapacityDate =
                _appClock.TurkeyToday;

            existingApplication.IsAvailable =
                false;

            existingApplication.IsApproved =
                false;

            existingApplication.VerificationStatus =
                ProducerVerificationStatuses.Pending;

            existingApplication.ApprovedAt = null;
            existingApplication.ApprovedByAdminId = null;

            existingApplication.RejectedAt = null;
            existingApplication.RejectedByAdminId = null;

            existingApplication.RejectionReason = null;

            existingApplication.CreatedAt =
                _appClock.UtcNow;

            existingApplication.CapacityVersion++;

            try
            {
                await _producerRepository
                    .SaveChangesAsync();

                if (
                    replacementBusinessImageUrl != null &&
                    !string.Equals(
                        previousBusinessImageUrl,
                        replacementBusinessImageUrl,
                        StringComparison.OrdinalIgnoreCase)
                )
                {
                    await SafeDeleteBusinessImageAsync(
                        previousBusinessImageUrl);
                }

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                await SafeDeleteBusinessImageAsync(
                    replacementBusinessImageUrl);

                return false;
            }
            catch
            {
                await SafeDeleteBusinessImageAsync(
                    replacementBusinessImageUrl);

                throw;
            }
        }

        public async Task<
            ProducerApplicationStatusResponse?>
            GetMyApplicationAsync(
                int userId)
        {
            var producerProfile =
                await _producerRepository
                    .GetByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return null;
            }

            return MapToResponse(
                producerProfile);
        }

        public async Task<
            ProducerApplicationStatusResponse?>
            UpdateMyProfileAsync(
                int userId,
                UpdateProducerProfileRequest request)
        {
            var businessName =
                request.BusinessName?.Trim() ??
                string.Empty;

            var description =
                request.Description?.Trim() ??
                string.Empty;

            var address =
                request.Address?.Trim() ??
                string.Empty;

            var city =
                request.City?.Trim() ??
                string.Empty;

            var district =
                request.District?.Trim() ??
                string.Empty;

            var neighborhood =
                request.Neighborhood?.Trim() ??
                string.Empty;

            var street =
                request.Street?.Trim() ??
                string.Empty;

            var buildingNo =
                request.BuildingNo?.Trim() ??
                string.Empty;

            var floor =
                NormalizeOptional(
                    request.Floor);

            var apartmentNo =
                NormalizeOptional(
                    request.ApartmentNo);

            var addressNote =
                NormalizeOptional(
                    request.AddressNote);

            if (!IsRequiredTextValid(
                    businessName,
                    2,
                    150) ||
                !IsRequiredTextValid(
                    description,
                    10,
                    1000) ||
                !IsRequiredTextValid(
                    address,
                    10,
                    500) ||
                !IsRequiredTextValid(
                    city,
                    1,
                    100) ||
                !IsRequiredTextValid(
                    district,
                    1,
                    100) ||
                !IsRequiredTextValid(
                    neighborhood,
                    1,
                    120) ||
                !IsRequiredTextValid(
                    street,
                    1,
                    150) ||
                !IsRequiredTextValid(
                    buildingNo,
                    1,
                    30))
            {
                return null;
            }

            if (!IsOptionalTextValid(
                    floor,
                    20) ||
                !IsOptionalTextValid(
                    apartmentNo,
                    20) ||
                !IsOptionalTextValid(
                    addressNote,
                    300))
            {
                return null;
            }

            if (
                !double.TryParse(
                    request.Latitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var latitude) ||
                latitude < -90 ||
                latitude > 90
            )
            {
                throw new ArgumentException(
                    "Enlem bilgisi geçersizdir.");
            }

            if (
                !double.TryParse(
                    request.Longitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var longitude) ||
                longitude < -180 ||
                longitude > 180
            )
            {
                throw new ArgumentException(
                    "Boylam bilgisi geçersizdir.");
            }

            if (request.DailyCapacity < 1 ||
                request.DailyCapacity > 1000)
            {
                return null;
            }

            var producerProfile =
                await _producerRepository
                    .GetByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return null;
            }

            if (!producerProfile.IsApproved ||
                !string.Equals(
                    producerProfile
                        .VerificationStatus,
                    ProducerVerificationStatuses
                        .Approved,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                return null;
            }

            var today =
                _appClock.TurkeyToday;

            int usedCapacity;

            if (producerProfile.CapacityDate !=
                today)
            {
                usedCapacity = 0;
            }
            else
            {
                usedCapacity =
                    Math.Max(
                        0,
                        producerProfile
                            .DailyCapacity -
                        producerProfile
                            .RemainingCapacity);
            }

            var newRemainingCapacity =
                Math.Max(
                    0,
                    request.DailyCapacity -
                    usedCapacity);

            var oldBusinessImageUrl =
                producerProfile.BusinessImageUrl;

            string? newBusinessImageUrl =
                null;

            if (
                request.BusinessImage != null &&
                request.BusinessImage.Length > 0
            )
            {
                newBusinessImageUrl =
                    await _producerImageStorageService
                        .SaveAsync(
                            request.BusinessImage);
            }

            producerProfile.BusinessName =
                businessName;

            producerProfile.Description =
                description;

            if (!string.IsNullOrWhiteSpace(
                    newBusinessImageUrl))
            {
                producerProfile.BusinessImageUrl =
                    newBusinessImageUrl;
            }

            producerProfile.Address =
                address;

            ApplyStructuredAddress(
                producerProfile,
                city,
                district,
                neighborhood,
                street,
                buildingNo,
                floor,
                apartmentNo,
                addressNote,
                latitude,
                longitude);

            producerProfile.DailyCapacity =
                request.DailyCapacity;

            producerProfile.RemainingCapacity =
                newRemainingCapacity;

            producerProfile.CapacityDate =
                today;

            producerProfile.IsAvailable =
                request.IsAvailable;

            producerProfile.CapacityVersion++;

            try
            {
                await _producerRepository
                    .SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!string.IsNullOrWhiteSpace(
                        newBusinessImageUrl))
                {
                    await SafeDeleteBusinessImageAsync(
                        newBusinessImageUrl);
                }

                return null;
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(
                        newBusinessImageUrl))
                {
                    await SafeDeleteBusinessImageAsync(
                        newBusinessImageUrl);
                }

                throw;
            }

            if (
                !string.IsNullOrWhiteSpace(
                    newBusinessImageUrl) &&
                !string.Equals(
                    oldBusinessImageUrl,
                    newBusinessImageUrl,
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                await SafeDeleteBusinessImageAsync(
                    oldBusinessImageUrl);
            }

            return MapToResponse(
                producerProfile);
        }

        public async Task<
            List<ProducerStorefrontSummaryResponse>>
            GetAvailableStorefrontsAsync(
                int? categoryId)
        {
            var storefronts =
                await _producerRepository
                    .GetAvailableStorefrontsAsync(
                        categoryId);

            return storefronts
                .Select(storefront =>
                    new ProducerStorefrontSummaryResponse
                    {
                        ProducerProfileId =
                            storefront
                                .ProducerProfileId,

                        BusinessName =
                            storefront.BusinessName,

                        Description =
                            storefront.Description,

                        BusinessImageUrl =
                            storefront
                                .BusinessImageUrl,

                        Rating =
                            storefront.Rating,

                        City =
                            storefront.City,

                        District =
                            storefront.District,

                        AvailableFoodCount =
                            storefront
                                .AvailableFoodCount,

                        AvailableCategoryCount =
                            storefront
                                .AvailableCategoryCount,

                        MatchingFoodCount =
                            storefront
                                .MatchingFoodCount,

                        MinimumPreparationTimeMinutes =
                            storefront
                                .MinimumPreparationTimeMinutes
                    })
                .ToList();
        }

        public async Task<
                    List<PopularProducerStorefrontResponse>>
                    GetPopularStorefrontsAsync(
                        int limit)
        {
            var safeLimit =
                Math.Clamp(
                    limit,
                    1,
                    20);

            var fromUtc =
                _appClock.UtcNow
                    .AddDays(-30);

            var candidates =
                await _producerRepository
                    .GetPopularityCandidatesAsync(
                        fromUtc);

            if (candidates.Count == 0)
            {
                return new List<
                    PopularProducerStorefrontResponse>();
            }

            var totalReviewCount =
                candidates.Sum(candidate =>
                    candidate.ReviewCount);

            var globalRatingMean =
                totalReviewCount > 0
                    ? candidates.Sum(candidate =>
                            (double)candidate.Rating *
                            candidate.ReviewCount) /
                        totalReviewCount
                    : 0.0;

            const double priorReviewWeight = 5.0;

            var maxDeliveredOrders =
                candidates.Max(candidate =>
                    candidate.DeliveredOrderCount30Days);

            var maxDistinctCustomers =
                candidates.Max(candidate =>
                    candidate.DistinctCustomerCount30Days);

            var maxFavoriteCount =
                candidates.Max(candidate =>
                    candidate.FavoriteCount);

            return candidates
                .Select(candidate =>
                {
                    var reviewCount =
                        candidate.ReviewCount;

                    var bayesianRating =
                        reviewCount > 0
                            ? (
                                (
                                    reviewCount *
                                    (double)candidate.Rating
                                ) +
                                (
                                    priorReviewWeight *
                                    globalRatingMean
                                )
                              ) /
                              (
                                  reviewCount +
                                  priorReviewWeight
                              )
                            : globalRatingMean;

                    var deliveredOrderScore =
                        NormalizePopularityMetric(
                            candidate.DeliveredOrderCount30Days,
                            maxDeliveredOrders);

                    var ratingScore =
                        Math.Clamp(
                            bayesianRating / 5.0,
                            0.0,
                            1.0);

                    var distinctCustomerScore =
                        NormalizePopularityMetric(
                            candidate.DistinctCustomerCount30Days,
                            maxDistinctCustomers);

                    var favoriteScore =
                        NormalizePopularityMetric(
                            candidate.FavoriteCount,
                            maxFavoriteCount);

                    var repeatCustomerRatio =
                        candidate.DistinctCustomerCount30Days > 0
                            ? (double)candidate.RepeatCustomerCount30Days /
                              candidate.DistinctCustomerCount30Days
                            : 0.0;

                    var popularityScore =
                        (
                            deliveredOrderScore * 0.45 +
                            ratingScore * 0.25 +
                            distinctCustomerScore * 0.15 +
                            favoriteScore * 0.10 +
                            repeatCustomerRatio * 0.05
                        ) * 100.0;

                    return new
                    {
                        Candidate = candidate,
                        PopularityScore =
                            Math.Round(
                                popularityScore,
                                2)
                    };
                })
                .OrderByDescending(item =>
                    item.PopularityScore)
                .ThenByDescending(item =>
                    item.Candidate.DeliveredOrderCount30Days)
                .ThenByDescending(item =>
                    item.Candidate.Rating)
                .ThenBy(item =>
                    item.Candidate.BusinessName)
                .Take(safeLimit)
                .Select(item =>
                    new PopularProducerStorefrontResponse
                    {
                        ProducerProfileId =
                            item.Candidate.ProducerProfileId,

                        BusinessName =
                            item.Candidate.BusinessName,

                        Description =
                            item.Candidate.Description,

                        BusinessImageUrl =
                            item.Candidate.BusinessImageUrl,

                        Rating =
                            item.Candidate.Rating,

                        City =
                            item.Candidate.City,

                        District =
                            item.Candidate.District,

                        AvailableFoodCount =
                            item.Candidate.AvailableFoodCount,

                        AvailableCategoryCount =
                            item.Candidate.AvailableCategoryCount,

                        MatchingFoodCount =
                            item.Candidate.AvailableFoodCount,

                        MinimumPreparationTimeMinutes =
                            item.Candidate
                                .MinimumPreparationTimeMinutes,

                        PopularityScore =
                            item.PopularityScore,

                        DeliveredOrderCount30Days =
                            item.Candidate.DeliveredOrderCount30Days,

                        DistinctCustomerCount30Days =
                            item.Candidate.DistinctCustomerCount30Days,

                        ReviewCount =
                            item.Candidate.ReviewCount,

                        FavoriteCount =
                            item.Candidate.FavoriteCount
                    })
                .ToList();
        }

        private static double NormalizePopularityMetric(
            int value,
            int maximum)
        {
            if (maximum <= 0 ||
                value <= 0)
            {
                return 0.0;
            }

            return Math.Clamp(
                (double)value / maximum,
                0.0,
                1.0);
        }

        public async Task<
            ProducerStorefrontMenuResponse?>
            GetAvailableStorefrontMenuAsync(
                int producerProfileId)
        {
            var storefront =
                await _producerRepository
                    .GetAvailableStorefrontMenuAsync(
                        producerProfileId);

            if (storefront == null)
            {
                return null;
            }

            /*

             * Kategoriler backend tarafında gruplanır.

             * Böylece Android Food listesini tekrar kategoriye

             * ayırmak zorunda kalmaz.

             *

             * Boş kategori üretilemez; yalnızca storefront.Foods

             * içinde gerçekten bulunan kategoriler response'a girer.

             */
            var categories =
                storefront.Foods
                    .GroupBy(food =>
                        new
                        {
                            food.CategoryId,
                            food.CategoryName
                        })
                    .OrderBy(group =>
                        group.Key.CategoryName)
                    .Select(group =>
                        new ProducerStorefrontMenuCategoryResponse
                        {
                            CategoryId =
                                group.Key.CategoryId,

                            CategoryName =
                                group.Key.CategoryName,

                            Foods =
                                group
                                    .OrderBy(food =>
                                        food.Name)
                                    .Select(food =>
                                        new ProducerStorefrontMenuFoodResponse
                                        {
                                            Id =
                                                food.Id,

                                            Name =
                                                food.Name,

                                            Description =
                                                food.Description,

                                            Price =
                                                food.Price,

                                            PreparationTimeMinutes =
                                                food
                                                    .PreparationTimeMinutes,

                                            ImageUrl =
                                                food.ImageUrl
                                        })
                                    .ToList()
                        })
                    .ToList();

            return new ProducerStorefrontMenuResponse
            {
                ProducerProfileId =
                    storefront.ProducerProfileId,

                BusinessName =
                    storefront.BusinessName,

                Description =
                    storefront.Description,

                BusinessImageUrl =
                    storefront.BusinessImageUrl,

                Rating =
                    storefront.Rating,

                City =
                    storefront.City,

                District =
                    storefront.District,

                AvailableFoodCount =
                    storefront.Foods.Count,

                AvailableCategoryCount =
                    categories.Count,

                Categories =
                    categories
            };
        }

        private async Task SafeDeleteBusinessImageAsync(
            string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            try
            {
                await _producerImageStorageService
                    .DeleteAsync(imageUrl);
            }
            catch
            {
                /*

                 * DB işlemi başarılı/başarısız durumdayken eski veya

                 * geçici dosyanın temizlenememesi ana isteği bozmaz.

                 * İleride merkezi loglama eklendiğinde burada loglanabilir.

                 */
            }
        }

        private static void ApplyStructuredAddress(
            ProducerProfile producerProfile,
            string city,
            string district,
            string neighborhood,
            string street,
            string buildingNo,
            string? floor,
            string? apartmentNo,
            string? addressNote,
            double latitude,
            double longitude)
        {
            producerProfile.City = city;
            producerProfile.District = district;
            producerProfile.Neighborhood = neighborhood;
            producerProfile.Street = street;
            producerProfile.BuildingNo = buildingNo;
            producerProfile.Floor = floor;
            producerProfile.ApartmentNo = apartmentNo;
            producerProfile.AddressNote = addressNote;
            producerProfile.Latitude = latitude;
            producerProfile.Longitude = longitude;
        }

        private static ProducerApplicationStatusResponse
            MapToResponse(
                ProducerProfile producerProfile)
        {
            return new ProducerApplicationStatusResponse
            {
                ProducerProfileId =
                    producerProfile.Id,

                BusinessName =
                    producerProfile.BusinessName,

                Description =
                    producerProfile.Description,

                BusinessImageUrl =
                    producerProfile.BusinessImageUrl,

                Address =
                    producerProfile.Address,

                City =
                    producerProfile.City,

                District =
                    producerProfile.District,

                Neighborhood =
                    producerProfile.Neighborhood,

                Street =
                    producerProfile.Street,

                BuildingNo =
                    producerProfile.BuildingNo,

                Floor =
                    producerProfile.Floor,

                ApartmentNo =
                    producerProfile.ApartmentNo,

                AddressNote =
                    producerProfile.AddressNote,

                Latitude =
                    producerProfile.Latitude,

                Longitude =
                    producerProfile.Longitude,

                DailyCapacity =
                    producerProfile.DailyCapacity,

                RemainingCapacity =
                    producerProfile.RemainingCapacity,

                IsAvailable =
                    producerProfile.IsAvailable,

                IsApproved =
                    producerProfile.IsApproved,

                VerificationStatus =
                    producerProfile.VerificationStatus,

                CreatedAt =
                    producerProfile.CreatedAt,

                ApprovedAt =
                    producerProfile.ApprovedAt,

                RejectedAt =
                    producerProfile.RejectedAt,

                RejectionReason =
                    producerProfile.RejectionReason
            };
        }

        private static bool IsRequiredTextValid(
            string value,
            int minimumLength,
            int maximumLength)
        {
            return !string.IsNullOrWhiteSpace(
                       value) &&
                   value.Length >= minimumLength &&
                   value.Length <= maximumLength;
        }

        private static string? NormalizeOptional(
            string? value)
        {
            var trimmed =
                value?.Trim();

            return string.IsNullOrWhiteSpace(
                trimmed)
                ? null
                : trimmed;
        }

        private static bool IsOptionalTextValid(
            string? value,
            int maximumLength)
        {
            return value == null ||
                   value.Length <= maximumLength;
        }
    }
}