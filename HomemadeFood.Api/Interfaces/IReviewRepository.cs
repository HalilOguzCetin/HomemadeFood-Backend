using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface IReviewRepository
    {
        Task AddAsync(Review review);

        Task<Review?> GetByOrderIdAsync(
            int orderId);

        Task<List<Review>> GetByCustomerIdAsync(
            int customerId);

        Task<List<Review>> GetByProducerProfileIdAsync(
            int producerProfileId);

        Task<Review?> GetTrackedByIdAndCustomerIdAsync(
            int reviewId,
            int customerId);

        Task<(decimal TotalRating, int ReviewCount)>
            GetRatingSummaryAsync(
                int producerProfileId);

        void Remove(Review review);

        Task SaveChangesAsync();
    }
}