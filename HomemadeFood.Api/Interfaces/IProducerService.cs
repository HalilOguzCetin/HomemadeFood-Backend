using HomemadeFood.Api.DTOs.Producer;

namespace HomemadeFood.Api.Interfaces
{
    public interface IProducerService
    {
        Task<bool> ApplyAsync(
            int userId,
            ProducerApplicationRequest request);

        Task<ProducerApplicationStatusResponse?>
            GetMyApplicationAsync(
                int userId);

        Task<ProducerApplicationStatusResponse?>
            UpdateMyProfileAsync(
                int userId,
                UpdateProducerProfileRequest request);

        Task<List<ProducerStorefrontSummaryResponse>>
            GetAvailableStorefrontsAsync(
                int? categoryId);

        Task<ProducerStorefrontMenuResponse?>
            GetAvailableStorefrontMenuAsync(
                int producerProfileId);
    }
}