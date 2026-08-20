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

        /*
         * H4B:
         * Gerçek kullanıcı davranışından hesaplanan
         * popüler işletme listesini döndürür.
         */
        Task<List<PopularProducerStorefrontResponse>>
            GetPopularStorefrontsAsync(
                int limit);

        Task<ProducerStorefrontMenuResponse?>
            GetAvailableStorefrontMenuAsync(
                int producerProfileId);
    }
}