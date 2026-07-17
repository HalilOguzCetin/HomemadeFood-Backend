using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Review;
using HomemadeFood.Api.Interfaces;
using ReviewEntity =
    HomemadeFood.Api.Entities.Review;

namespace HomemadeFood.Api.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository
            _reviewRepository;

        private readonly IOrderRepository
            _orderRepository;

        private readonly IAppClock
            _appClock;

        public ReviewService(
            IReviewRepository reviewRepository,
            IOrderRepository orderRepository,
            IAppClock appClock)
        {
            _reviewRepository =
                reviewRepository;

            _orderRepository =
                orderRepository;

            _appClock =
                appClock;
        }

        public async Task<ReviewResponse?>
            CreateReviewAsync(
                int customerId,
                CreateReviewRequest request)
        {
            var order =
                await _orderRepository
                    .GetTrackedByIdAndCustomerIdAsync(
                        request.OrderId,
                        customerId);

            if (order == null)
            {
                return null;
            }

            // Yalnızca teslim edilmiş siparişler
            // değerlendirilebilir.
            if (!string.Equals(
                    order.Status,
                    OrderStatuses.Delivered,
                    StringComparison.Ordinal))
            {
                return null;
            }

            // Ek güvenlik kontrolü.
            if (request.Rating < 1 ||
                request.Rating > 5)
            {
                return null;
            }

            // Aynı sipariş daha önce
            // değerlendirilmiş mi?
            var existingReview =
                await _reviewRepository
                    .GetByOrderIdAsync(order.Id);

            if (existingReview != null)
            {
                return null;
            }

            /*
             * Yeni yorum henüz veritabanına eklenmeden
             * önce mevcut puan toplamı ve yorum sayısı
             * alınır.
             */
            var (
                totalRating,
                reviewCount) =
                await _reviewRepository
                    .GetRatingSummaryAsync(
                        order.ProducerProfileId);

            var newReviewCount =
                reviewCount + 1;

            var newTotalRating =
                totalRating + request.Rating;

            var newAverageRating =
                Math.Round(
                    newTotalRating /
                    newReviewCount,
                    2,
                    MidpointRounding.AwayFromZero);

            var review =
                new ReviewEntity
                {
                    OrderId =
                        order.Id,

                    Order =
                        order,

                    CustomerId =
                        customerId,

                    Customer =
                        order.Customer,

                    ProducerProfileId =
                        order.ProducerProfileId,

                    ProducerProfile =
                        order.ProducerProfile,

                    Rating =
                        request.Rating,

                    Comment =
                        request.Comment?.Trim()
                        ?? string.Empty,

                    CreatedAt =
                        _appClock.UtcNow
                };

            // Yorum eklenmek üzere takip edilir.
            await _reviewRepository
                .AddAsync(review);

            // Üretici puanı aynı işlemde güncellenir.
            order.ProducerProfile.Rating =
                newAverageRating;

            /*
             * Tek SaveChangesAsync ile:
             *
             * - Yorum ekleme
             * - Üretici puanı güncelleme
             *
             * birlikte kaydedilir.
             */
            await _reviewRepository
                .SaveChangesAsync();

            return MapToResponse(review);
        }

        public async Task<List<ReviewResponse>>
            GetMyReviewsAsync(
                int customerId)
        {
            var reviews =
                await _reviewRepository
                    .GetByCustomerIdAsync(
                        customerId);

            return reviews
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<List<ReviewResponse>>
            GetProducerReviewsAsync(
                int producerProfileId)
        {
            var reviews =
                await _reviewRepository
                    .GetByProducerProfileIdAsync(
                        producerProfileId);

            return reviews
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<bool>
            DeleteReviewAsync(
                int customerId,
                int reviewId)
        {
            var review =
                await _reviewRepository
                    .GetTrackedByIdAndCustomerIdAsync(
                        reviewId,
                        customerId);

            if (review == null)
            {
                return false;
            }

            var producerProfile =
                review.ProducerProfile;

            var producerProfileId =
                review.ProducerProfileId;

            /*
             * Mevcut toplam ve yorum sayısına
             * silinecek yorum da dahildir.
             */
            var (
                totalRating,
                reviewCount) =
                await _reviewRepository
                    .GetRatingSummaryAsync(
                        producerProfileId);

            var remainingReviewCount =
                Math.Max(
                    0,
                    reviewCount - 1);

            decimal newAverageRating;

            if (remainingReviewCount == 0)
            {
                newAverageRating = 0m;
            }
            else
            {
                var remainingTotalRating =
                    Math.Max(
                        0m,
                        totalRating -
                        review.Rating);

                newAverageRating =
                    Math.Round(
                        remainingTotalRating /
                        remainingReviewCount,
                        2,
                        MidpointRounding.AwayFromZero);
            }

            // Yorum silinmek üzere işaretlenir.
            _reviewRepository.Remove(review);

            // Üretici puanı aynı işlemde güncellenir.
            producerProfile.Rating =
                newAverageRating;

            /*
             * Tek SaveChangesAsync ile:
             *
             * - Yorum silme
             * - Üretici puanı güncelleme
             *
             * birlikte kaydedilir.
             */
            await _reviewRepository
                .SaveChangesAsync();

            return true;
        }

        private static ReviewResponse MapToResponse(
            ReviewEntity review)
        {
            return new ReviewResponse
            {
                ReviewId =
                    review.Id,

                OrderId =
                    review.OrderId,

                ProducerProfileId =
                    review.ProducerProfileId,

                BusinessName =
                    review.ProducerProfile
                        .BusinessName,

                CustomerFullName =
                    review.Customer.FullName,

                Rating =
                    review.Rating,

                Comment =
                    review.Comment,

                CreatedAt =
                    review.CreatedAt
            };
        }
    }
}