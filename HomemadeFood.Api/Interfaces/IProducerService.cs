using HomemadeFood.Api.DTOs.Producer;
using HomemadeFood.Api.DTOs.Common;

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

        Task<List<PopularProducerStorefrontResponse>>
            GetPopularStorefrontsAsync(
                int limit);

        Task<List<NearbyProducerStorefrontResponse>>
            GetNearbyStorefrontsAsync(
                double latitude,
                double longitude,
                double radiusKm,
                int limit);
        Task<
            PagedResultResponse<
                DiscoverProducerStorefrontResponse>>
            GetDiscoverStorefrontsAsync(
                double latitude,
                double longitude,
                string city,
                double radiusKm,
                int page,
                int pageSize,
                int? categoryId,
                string? search);
        Task<List<DiscoverProducerStorefrontResponse>>
           GetCityStorefrontsAsync(
               string city,
               double latitude,
               double longitude,
               int limit);

        Task<ProducerStorefrontMenuResponse?>
            GetAvailableStorefrontMenuAsync(
                int producerProfileId);
    }
}