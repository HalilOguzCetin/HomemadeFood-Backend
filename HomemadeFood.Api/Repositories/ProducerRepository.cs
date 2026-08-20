using HomemadeFood.Api.Data;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using HomemadeFood.Api.Constants;
using HomemadeFood.Api.ReadModels;

namespace HomemadeFood.Api.Repositories
{
    public class ProducerRepository : IProducerRepository
    {
        private readonly AppDbContext _context;

        public ProducerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ProducerProfile producerProfile)
        {
            await _context.ProducerProfiles.AddAsync(producerProfile);
        }

        public async Task<bool> HasApplicationAsync(int userId)
        {
            return await _context.ProducerProfiles
                .AnyAsync(x => x.UserId == userId);
        }
        public async Task<ProducerProfile?>
    GetByUserIdAsync(
        int userId)
        {
            return await _context.ProducerProfiles
                .FirstOrDefaultAsync(
                    x => x.UserId == userId);
        }
        public async Task<List<ProducerProfile>> GetPendingApplicationsAsync()
        {
            return await _context.ProducerProfiles
                .Include(x => x.User)
                .Where(x => x.VerificationStatus ==
    ProducerVerificationStatuses.Pending)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }
        public async Task<List<ProducerProfile>>
    GetApplicationsByStatusAsync(
        string verificationStatus)
        {
            return await _context
                .ProducerProfiles
                .AsNoTracking()
                .Include(producerProfile =>
                    producerProfile.User)
                .Where(producerProfile =>
                    producerProfile.VerificationStatus ==
                    verificationStatus)
                .OrderByDescending(producerProfile =>
                    producerProfile.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProducerProfile?> GetByIdWithUserAsync(int producerProfileId)
        {
            return await _context.ProducerProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == producerProfileId);
        }
        public async Task<ProducerProfile?>
    GetApprovedByUserIdAsync(int userId)
        {
            return await _context.ProducerProfiles
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.IsApproved &&
                    x.VerificationStatus ==
    ProducerVerificationStatuses.Approved);
        }

        public async Task<
            List<ProducerStorefrontSummaryReadModel>>
            GetAvailableStorefrontsAsync(
                int? categoryId)
        {
            /*
             * İşletme listesi ProducerProfiles üzerinden başlar.
             * Bu sayede aynı işletme bir kategoride birden fazla
             * yemek bulundursa bile yalnızca bir kez döner.
             */
            var producerQuery =
                _context.ProducerProfiles
                    .AsNoTracking()
                    .Where(producerProfile =>
                        producerProfile.IsApproved &&
                        producerProfile.IsAvailable &&
                        producerProfile
                            .VerificationStatus ==
                        ProducerVerificationStatuses
                            .Approved);

            if (categoryId.HasValue)
            {
                var selectedCategoryId =
                    categoryId.Value;

                producerQuery =
                    producerQuery.Where(
                        producerProfile =>
                            producerProfile.Foods
                                .Any(food =>
                                    food.IsAvailable &&
                                    food.Category.IsActive &&
                                    food.CategoryId ==
                                        selectedCategoryId));

                return await producerQuery
                    .Select(producerProfile =>
                        new ProducerStorefrontSummaryReadModel
                        {
                            ProducerProfileId =
                                producerProfile.Id,

                            BusinessName =
                                producerProfile
                                    .BusinessName,

                            Description =
                                producerProfile
                                    .Description,

                            BusinessImageUrl =
                                producerProfile
                                    .BusinessImageUrl,

                            Rating =
                                producerProfile.Rating,

                            City =
                                producerProfile.City,

                            District =
                                producerProfile.District,

                            AvailableFoodCount =
                                producerProfile.Foods
                                    .Count(food =>
                                        food.IsAvailable &&
                                        food.Category
                                            .IsActive),

                            AvailableCategoryCount =
                                producerProfile.Foods
                                    .Where(food =>
                                        food.IsAvailable &&
                                        food.Category
                                            .IsActive)
                                    .Select(food =>
                                        food.CategoryId)
                                    .Distinct()
                                    .Count(),

                            MatchingFoodCount =
                                producerProfile.Foods
                                    .Count(food =>
                                        food.IsAvailable &&
                                        food.Category
                                            .IsActive &&
                                        food.CategoryId ==
                                            selectedCategoryId),

                            MinimumPreparationTimeMinutes =
                                producerProfile.Foods
                                    .Where(food =>
                                        food.IsAvailable &&
                                        food.Category
                                            .IsActive &&
                                        food.CategoryId ==
                                            selectedCategoryId)
                                    .Select(food =>
                                        (int?)
                                            food
                                                .PreparationTimeMinutes)
                                    .Min()
                        })
                    .OrderByDescending(storefront =>
                        storefront.Rating)
                    .ThenByDescending(storefront =>
                        storefront.MatchingFoodCount)
                    .ThenBy(storefront =>
                        storefront.BusinessName)
                    .ToListAsync();
            }

            /*
             * Tümü filtresinde en az bir satışta ve aktif
             * kategoride yemeği bulunan işletmeler döner.
             */
            producerQuery =
                producerQuery.Where(
                    producerProfile =>
                        producerProfile.Foods
                            .Any(food =>
                                food.IsAvailable &&
                                food.Category.IsActive));

            return await producerQuery
                .Select(producerProfile =>
                    new ProducerStorefrontSummaryReadModel
                    {
                        ProducerProfileId =
                            producerProfile.Id,

                        BusinessName =
                            producerProfile.BusinessName,

                        Description =
                            producerProfile.Description,

                        BusinessImageUrl =
                            producerProfile
                                .BusinessImageUrl,

                        Rating =
                            producerProfile.Rating,

                        City =
                            producerProfile.City,

                        District =
                            producerProfile.District,

                        AvailableFoodCount =
                            producerProfile.Foods
                                .Count(food =>
                                    food.IsAvailable &&
                                    food.Category.IsActive),

                        AvailableCategoryCount =
                            producerProfile.Foods
                                .Where(food =>
                                    food.IsAvailable &&
                                    food.Category.IsActive)
                                .Select(food =>
                                    food.CategoryId)
                                .Distinct()
                                .Count(),

                        MatchingFoodCount =
                            producerProfile.Foods
                                .Count(food =>
                                    food.IsAvailable &&
                                    food.Category.IsActive),

                        MinimumPreparationTimeMinutes =
                            producerProfile.Foods
                                .Where(food =>
                                    food.IsAvailable &&
                                    food.Category.IsActive)
                                .Select(food =>
                                    (int?)
                                        food
                                            .PreparationTimeMinutes)
                                .Min()
                    })
                .OrderByDescending(storefront =>
                    storefront.Rating)
                .ThenByDescending(storefront =>
                    storefront.AvailableFoodCount)
                .ThenBy(storefront =>
                    storefront.BusinessName)
                .ToListAsync();
        }


        public async Task<
            List<ProducerPopularityCandidateReadModel>>
            GetPopularityCandidatesAsync(
                DateTime fromUtc)
        {
            /*
             * Önce müşteriye gerçekten gösterilebilir işletmeleri
             * ve statik storefront metriklerini alıyoruz.
             */
            var candidates =
                await _context.ProducerProfiles
                    .AsNoTracking()
                    .Where(producerProfile =>
                        producerProfile.IsApproved &&
                        producerProfile.IsAvailable &&
                        producerProfile
                            .VerificationStatus ==
                        ProducerVerificationStatuses
                            .Approved &&
                        producerProfile.Foods
                            .Any(food =>
                                food.IsAvailable &&
                                food.Category.IsActive))
                    .Select(producerProfile =>
                        new ProducerPopularityCandidateReadModel
                        {
                            ProducerProfileId =
                                producerProfile.Id,

                            BusinessName =
                                producerProfile.BusinessName,

                            Description =
                                producerProfile.Description,

                            BusinessImageUrl =
                                producerProfile
                                    .BusinessImageUrl,

                            Rating =
                                producerProfile.Rating,

                            City =
                                producerProfile.City,

                            District =
                                producerProfile.District,

                            AvailableFoodCount =
                                producerProfile.Foods
                                    .Count(food =>
                                        food.IsAvailable &&
                                        food.Category
                                            .IsActive),

                            AvailableCategoryCount =
                                producerProfile.Foods
                                    .Where(food =>
                                        food.IsAvailable &&
                                        food.Category
                                            .IsActive)
                                    .Select(food =>
                                        food.CategoryId)
                                    .Distinct()
                                    .Count(),

                            MinimumPreparationTimeMinutes =
                                producerProfile.Foods
                                    .Where(food =>
                                        food.IsAvailable &&
                                        food.Category
                                            .IsActive)
                                    .Select(food =>
                                        (int?)
                                            food
                                                .PreparationTimeMinutes)
                                    .Min(),

                            ReviewCount =
                                producerProfile.Reviews
                                    .Count(),

                            FavoriteCount =
                                producerProfile.Foods
                                    .SelectMany(food =>
                                        food.Favorites)
                                    .Count()
                        })
                    .ToListAsync();

            if (candidates.Count == 0)
            {
                return candidates;
            }

            var producerProfileIds =
                candidates
                    .Select(candidate =>
                        candidate.ProducerProfileId)
                    .ToList();

            /*
             * Popülerlikte yalnızca gerçekten tamamlanmış
             * Delivered siparişler kullanılır.
             *
             * Cancelled/Rejected/Pending vb. durumlar sinyale
             * hiçbir şekilde dahil edilmez.
             */
            var deliveredOrders =
                await _context.Orders
                    .AsNoTracking()
                    .Where(order =>
                        producerProfileIds.Contains(
                            order.ProducerProfileId) &&
                        order.Status ==
                            OrderStatuses.Delivered &&
                        order.CreatedAt >= fromUtc)
                    .Select(order =>
                        new
                        {
                            order.ProducerProfileId,
                            order.CustomerId
                        })
                    .ToListAsync();

            var orderMetrics =
                deliveredOrders
                    .GroupBy(order =>
                        order.ProducerProfileId)
                    .ToDictionary(
                        group => group.Key,
                        group =>
                        {
                            var customerOrderCounts =
                                group
                                    .GroupBy(order =>
                                        order.CustomerId)
                                    .Select(customerGroup =>
                                        customerGroup.Count())
                                    .ToList();

                            return new
                            {
                                DeliveredOrderCount =
                                    group.Count(),

                                DistinctCustomerCount =
                                    customerOrderCounts.Count,

                                RepeatCustomerCount =
                                    customerOrderCounts
                                        .Count(count =>
                                            count >= 2)
                            };
                        });

            foreach (var candidate in candidates)
            {
                if (
                    !orderMetrics.TryGetValue(
                        candidate.ProducerProfileId,
                        out var metrics)
                )
                {
                    continue;
                }

                candidate.DeliveredOrderCount30Days =
                    metrics.DeliveredOrderCount;

                candidate.DistinctCustomerCount30Days =
                    metrics.DistinctCustomerCount;

                candidate.RepeatCustomerCount30Days =
                    metrics.RepeatCustomerCount;
            }

            return candidates;
        }

        public async Task<ProducerStorefrontMenuReadModel?>
            GetAvailableStorefrontMenuAsync(
                int producerProfileId)
        {
            /*
             * Önce yalnızca müşteriye açık ve kullanılabilir
             * işletmenin vitrin bilgilerini alıyoruz.
             *
             * İşletmenin hiç aktif yemeği yoksa menu endpoint'i
             * boş vitrin döndürmez; storefront listesiyle aynı
             * görünürlük kuralını korur.
             */
            var storefront =
                await _context.ProducerProfiles
                    .AsNoTracking()
                    .Where(producerProfile =>
                        producerProfile.Id ==
                            producerProfileId &&
                        producerProfile.IsApproved &&
                        producerProfile.IsAvailable &&
                        producerProfile
                            .VerificationStatus ==
                        ProducerVerificationStatuses
                            .Approved &&
                        producerProfile.Foods
                            .Any(food =>
                                food.IsAvailable &&
                                food.Category.IsActive))
                    .Select(producerProfile =>
                        new ProducerStorefrontMenuReadModel
                        {
                            ProducerProfileId =
                                producerProfile.Id,

                            BusinessName =
                                producerProfile.BusinessName,

                            Description =
                                producerProfile.Description,

                            BusinessImageUrl =
                                producerProfile
                                    .BusinessImageUrl,

                            Rating =
                                producerProfile.Rating,

                            City =
                                producerProfile.City,

                            District =
                                producerProfile.District
                        })
                    .FirstOrDefaultAsync();

            if (storefront == null)
            {
                return null;
            }

            /*
             * Menü için yalnızca aktif kategorilerde bulunan,
             * satışta olan yemekları getiriyoruz.
             *
             * Entity grafiğini komple yüklemek yerine gereken
             * alanları projection ile alıyoruz.
             */
            storefront.Foods =
                await _context.Foods
                    .AsNoTracking()
                    .Where(food =>
                        food.ProducerProfileId ==
                            producerProfileId &&
                        food.IsAvailable &&
                        food.Category.IsActive)
                    .Select(food =>
                        new ProducerStorefrontMenuFoodReadModel
                        {
                            Id =
                                food.Id,

                            CategoryId =
                                food.CategoryId,

                            CategoryName =
                                food.Category.Name,

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
                    .OrderBy(food =>
                        food.CategoryName)
                    .ThenBy(food =>
                        food.Name)
                    .ToListAsync();

            if (storefront.Foods.Count == 0)
            {
                return null;
            }

            return storefront;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}