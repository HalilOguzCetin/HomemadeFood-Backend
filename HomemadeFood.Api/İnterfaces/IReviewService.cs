using HomemadeFood.Api.DTOs.Review;

namespace HomemadeFood.Api.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewResponse?> CreateReviewAsync(
            int customerId,
            CreateReviewRequest request);

        Task<List<ReviewResponse>> GetMyReviewsAsync(
            int customerId);

        Task<List<ReviewResponse>>
            GetProducerReviewsAsync(
                int producerProfileId);

        Task<bool> DeleteReviewAsync(
            int customerId,
            int reviewId);
    }
}