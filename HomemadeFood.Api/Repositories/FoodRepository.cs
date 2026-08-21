using HomemadeFood.Api.Constants;
using HomemadeFood.Api.Data;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using HomemadeFood.Api.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Repositories
{
    public class FoodRepository : IFoodRepository
    {
        private readonly AppDbContext _context;

        public FoodRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Food food)
        {
            await _context.Foods
                .AddAsync(food);
        }

        public async Task<List<Food>>
            GetAvailableFoodsAsync(
                int? categoryId,
                string? search)
        {
            var query =
                _context.Foods
                    .AsNoTracking()
                    .Include(x => x.Category)
                    .Include(x =>
                        x.ProducerProfile)
                    .Where(x =>
                        x.IsAvailable &&
                        x.Category.IsActive &&
                        x.ProducerProfile
                            .IsApproved &&
                        x.ProducerProfile
                            .IsAvailable &&
                        x.ProducerProfile
                            .VerificationStatus ==
                        ProducerVerificationStatuses
                            .Approved)
                    .AsQueryable();

            if (categoryId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.CategoryId ==
                        categoryId.Value);
            }

            if (
                !string.IsNullOrWhiteSpace(
                    search)
            )
            {
                var searchValue =
                    search.Trim();

                query =
                    query.Where(x =>
                        EF.Functions.Like(
                            x.Name,
                            $"%{searchValue}%") ||
                        EF.Functions.Like(
                            x.Description,
                            $"%{searchValue}%"));
            }

            return await query
                .OrderByDescending(x =>
                    x.CreatedAt)
                .ToListAsync();
        }
        public async Task<
            List<FoodDiscoverCandidateReadModel>>
            GetDiscoverCandidatesAsync(
                int? categoryId,
                string? search,
                double minimumLatitude,
                double maximumLatitude,
                double minimumLongitude,
                double maximumLongitude,
                DateTime fromUtc)
        {
            var query =
                _context.Foods
                    .AsNoTracking()
                    .Where(food =>
                        food.IsAvailable &&
                        food.Category.IsActive &&
                        food.ProducerProfile
                            .IsApproved &&
                        food.ProducerProfile
                            .IsAvailable &&
                        food.ProducerProfile
                            .VerificationStatus ==
                        ProducerVerificationStatuses
                            .Approved &&
                        food.ProducerProfile.Latitude >=
                            minimumLatitude &&
                        food.ProducerProfile.Latitude <=
                            maximumLatitude &&
                        food.ProducerProfile.Longitude >=
                            minimumLongitude &&
                        food.ProducerProfile.Longitude <=
                            maximumLongitude);

            if (categoryId.HasValue)
            {
                query =
                    query.Where(food =>
                        food.CategoryId ==
                        categoryId.Value);
            }

            if (
                !string.IsNullOrWhiteSpace(
                    search)
            )
            {
                var searchValue =
                    search.Trim();

                query =
                    query.Where(food =>
                        EF.Functions.Like(
                            food.Name,
                            $"%{searchValue}%") ||
                        EF.Functions.Like(
                            food.Description,
                            $"%{searchValue}%") ||
                        EF.Functions.Like(
                            food.ProducerProfile
                                .BusinessName,
                            $"%{searchValue}%"));
            }

            var candidates =
                await query
                    .Select(food =>
                        new FoodDiscoverCandidateReadModel
                        {
                            Id =
                                food.Id,

                            ProducerProfileId =
                                food.ProducerProfileId,

                            BusinessName =
                                food.ProducerProfile
                                    .BusinessName,

                            ProducerCity =
                                food.ProducerProfile
                                    .City,

                            ProducerLatitude =
                                food.ProducerProfile
                                    .Latitude,

                            ProducerLongitude =
                                food.ProducerProfile
                                    .Longitude,

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
                                food.ImageUrl,

                            IsAvailable =
                                food.IsAvailable,

                            CreatedAt =
                                food.CreatedAt,

                            FavoriteCount =
                                food.Favorites.Count()
                        })
                    .ToListAsync();

            if (candidates.Count == 0)
            {
                return candidates;
            }

            var foodIds =
                candidates
                    .Select(candidate =>
                        candidate.Id)
                    .ToList();

            /*
             * H5 ile aynı davranış sinyali:
             * yalnız son 30 gündeki Delivered siparişler.
             */
            var deliveredItems =
                await _context.OrderItems
                    .AsNoTracking()
                    .Where(orderItem =>
                        foodIds.Contains(
                            orderItem.FoodId) &&
                        orderItem.Order.Status ==
                            OrderStatuses.Delivered &&
                        orderItem.Order.CreatedAt >=
                            fromUtc)
                    .Select(orderItem =>
                        new
                        {
                            orderItem.FoodId,
                            orderItem.OrderId,
                            orderItem.Quantity,

                            CustomerId =
                                orderItem.Order
                                    .CustomerId
                        })
                    .ToListAsync();

            var orderMetrics =
                deliveredItems
                    .GroupBy(item =>
                        item.FoodId)
                    .ToDictionary(
                        group => group.Key,
                        group =>
                        {
                            var customerOrderCounts =
                                group
                                    .GroupBy(item =>
                                        item.CustomerId)
                                    .Select(
                                        customerGroup =>
                                            customerGroup
                                                .Select(item =>
                                                    item.OrderId)
                                                .Distinct()
                                                .Count())
                                    .ToList();

                            return new
                            {
                                DeliveredOrderCount =
                                    group
                                        .Select(item =>
                                            item.OrderId)
                                        .Distinct()
                                        .Count(),

                                SoldQuantity =
                                    group.Sum(item =>
                                        item.Quantity),

                                DistinctCustomerCount =
                                    customerOrderCounts
                                        .Count,

                                RepeatCustomerCount =
                                    customerOrderCounts
                                        .Count(count =>
                                            count >= 2)
                            };
                        });

            foreach (
                var candidate in candidates
            )
            {
                if (
                    !orderMetrics.TryGetValue(
                        candidate.Id,
                        out var metrics)
                )
                {
                    continue;
                }

                candidate
                    .DeliveredOrderCount30Days =
                    metrics.DeliveredOrderCount;

                candidate
                    .SoldQuantity30Days =
                    metrics.SoldQuantity;

                candidate
                    .DistinctCustomerCount30Days =
                    metrics.DistinctCustomerCount;

                candidate
                    .RepeatCustomerCount30Days =
                    metrics.RepeatCustomerCount;
            }

            return candidates;
        }

        public async Task<
            List<FoodPopularityCandidateReadModel>>
            GetPopularityCandidatesAsync(
                DateTime fromUtc)
        {
            /*
             * Önce müşteriye gerçekten gösterilebilir
             * yemekleri ve statik alanlarını alıyoruz.
             */
            var candidates =
                await _context.Foods
                    .AsNoTracking()
                    .Where(food =>
                        food.IsAvailable &&
                        food.Category.IsActive &&
                        food.ProducerProfile
                            .IsApproved &&
                        food.ProducerProfile
                            .IsAvailable &&
                        food.ProducerProfile
                            .VerificationStatus ==
                        ProducerVerificationStatuses
                            .Approved)
                    .Select(food =>
                        new FoodPopularityCandidateReadModel
                        {
                            Id =
                                food.Id,

                            ProducerProfileId =
                                food.ProducerProfileId,

                            BusinessName =
                                food.ProducerProfile
                                    .BusinessName,

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
                                food.ImageUrl,

                            IsAvailable =
                                food.IsAvailable,

                            CreatedAt =
                                food.CreatedAt,

                            FavoriteCount =
                                food.Favorites.Count()
                        })
                    .ToListAsync();

            if (candidates.Count == 0)
            {
                return candidates;
            }

            var foodIds =
                candidates
                    .Select(candidate =>
                        candidate.Id)
                    .ToList();

            /*
             * Yalnız son 30 günde Delivered olmuş
             * siparişlerin OrderItem kayıtları popülerliğe
             * katılır.
             *
             * Pending / Preparing / Cancelled vb.
             * durumlar kesinlikle hesaba girmez.
             */
            var deliveredItems =
                await _context.OrderItems
                    .AsNoTracking()
                    .Where(orderItem =>
                        foodIds.Contains(
                            orderItem.FoodId) &&
                        orderItem.Order.Status ==
                            OrderStatuses.Delivered &&
                        orderItem.Order.CreatedAt >=
                            fromUtc)
                    .Select(orderItem =>
                        new
                        {
                            orderItem.FoodId,
                            orderItem.OrderId,
                            orderItem.Quantity,

                            CustomerId =
                                orderItem.Order
                                    .CustomerId
                        })
                    .ToListAsync();

            var orderMetrics =
                deliveredItems
                    .GroupBy(item =>
                        item.FoodId)
                    .ToDictionary(
                        group => group.Key,
                        group =>
                        {
                            var customerOrderCounts =
                                group
                                    .GroupBy(item =>
                                        item.CustomerId)
                                    .Select(
                                        customerGroup =>
                                            customerGroup
                                                .Select(item =>
                                                    item.OrderId)
                                                .Distinct()
                                                .Count())
                                    .ToList();

                            return new
                            {
                                DeliveredOrderCount =
                                    group
                                        .Select(item =>
                                            item.OrderId)
                                        .Distinct()
                                        .Count(),

                                SoldQuantity =
                                    group.Sum(item =>
                                        item.Quantity),

                                DistinctCustomerCount =
                                    customerOrderCounts
                                        .Count,

                                RepeatCustomerCount =
                                    customerOrderCounts
                                        .Count(count =>
                                            count >= 2)
                            };
                        });

            foreach (
                var candidate in candidates
            )
            {
                if (
                    !orderMetrics.TryGetValue(
                        candidate.Id,
                        out var metrics)
                )
                {
                    continue;
                }

                candidate
                    .DeliveredOrderCount30Days =
                    metrics.DeliveredOrderCount;

                candidate
                    .SoldQuantity30Days =
                    metrics.SoldQuantity;

                candidate
                    .DistinctCustomerCount30Days =
                    metrics.DistinctCustomerCount;

                candidate
                    .RepeatCustomerCount30Days =
                    metrics.RepeatCustomerCount;
            }

            return candidates;
        }

        public async Task<Food?>
            GetAvailableFoodByIdAsync(
                int foodId)
        {
            return await _context.Foods
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x =>
                    x.ProducerProfile)
                .FirstOrDefaultAsync(x =>
                    x.Id == foodId &&
                    x.IsAvailable &&
                    x.Category.IsActive &&
                    x.ProducerProfile
                        .IsApproved &&
                    x.ProducerProfile
                        .IsAvailable &&
                    x.ProducerProfile
                        .VerificationStatus ==
                    ProducerVerificationStatuses
                        .Approved);
        }

        public async Task<List<Food>>
            GetByProducerProfileIdAsync(
                int producerProfileId)
        {
            return await _context.Foods
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x =>
                    x.ProducerProfile)
                .Where(x =>
                    x.ProducerProfileId ==
                    producerProfileId)
                .OrderByDescending(x =>
                    x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Food?>
            GetByIdAndProducerProfileIdAsync(
                int foodId,
                int producerProfileId)
        {
            return await _context.Foods
                .Include(x => x.Category)
                .Include(x =>
                    x.ProducerProfile)
                .FirstOrDefaultAsync(x =>
                    x.Id == foodId &&
                    x.ProducerProfileId ==
                    producerProfileId);
        }

        public async Task<Category?>
            GetActiveCategoryByIdAsync(
                int categoryId)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.Id == categoryId &&
                    x.IsActive);
        }

        public async Task SaveChangesAsync()
        {
            await _context
                .SaveChangesAsync();
        }
    }
}