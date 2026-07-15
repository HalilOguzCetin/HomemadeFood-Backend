using HomemadeFood.Api.DTOs.Review;
using HomemadeFood.Api.Interfaces;
using ReviewEntity = HomemadeFood.Api.Entities.Review;

namespace HomemadeFood.Api.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IOrderRepository _orderRepository;

        public ReviewService(
            IReviewRepository reviewRepository,
            IOrderRepository orderRepository)
        {
            _reviewRepository = reviewRepository;
            _orderRepository = orderRepository;
        }

        public async Task<ReviewResponse?> CreateReviewAsync(
            int customerId,
            CreateReviewRequest request)
        {
            // Sipariş gerçekten giriş yapan müşteriye mi ait?
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
                    "Delivered",
                    StringComparison.Ordinal))
            {
                return null;
            }

            // Aynı sipariş daha önce değerlendirilmiş mi?
            var existingReview =
                await _reviewRepository
                    .GetByOrderIdAsync(order.Id);

            if (existingReview != null)
            {
                return null;
            }

            var review = new ReviewEntity
            {
                OrderId = order.Id,
                Order = order,

                CustomerId = customerId,
                Customer = order.Customer,

                ProducerProfileId =
                    order.ProducerProfileId,

                ProducerProfile =
                    order.ProducerProfile,

                Rating = request.Rating,

                Comment =
                    request.Comment?.Trim()
                    ?? string.Empty,

                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            await _reviewRepository.SaveChangesAsync();

            // Yeni değerlendirme eklendikten sonra
            // üreticinin puanını yeniden hesapla.
            var averageRating =
                await _reviewRepository
                    .GetAverageRatingAsync(
                        order.ProducerProfileId);

            order.ProducerProfile.Rating =
                Math.Round(averageRating, 2);

            await _reviewRepository.SaveChangesAsync();

            return MapToResponse(review);
        }

        public async Task<List<ReviewResponse>>
            GetMyReviewsAsync(int customerId)
        {
            var reviews =
                await _reviewRepository
                    .GetByCustomerIdAsync(customerId);

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

        public async Task<bool> DeleteReviewAsync(
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

            _reviewRepository.Remove(review);

            await _reviewRepository.SaveChangesAsync();

            // Değerlendirme silinince ortalama puanı
            // kalan yorumlara göre yeniden hesapla.
            var averageRating =
                await _reviewRepository
                    .GetAverageRatingAsync(
                        producerProfileId);

            producerProfile.Rating =
                Math.Round(averageRating, 2);

            await _reviewRepository.SaveChangesAsync();

            return true;
        }

        private static ReviewResponse MapToResponse(
            ReviewEntity review)
        {
            return new ReviewResponse
            {
                ReviewId = review.Id,

                OrderId = review.OrderId,

                ProducerProfileId =
                    review.ProducerProfileId,

                BusinessName =
                    review.ProducerProfile.BusinessName,

                CustomerFullName =
                    review.Customer.FullName,

                Rating = review.Rating,

                Comment = review.Comment,

                CreatedAt = review.CreatedAt
            };
        }
    }
}