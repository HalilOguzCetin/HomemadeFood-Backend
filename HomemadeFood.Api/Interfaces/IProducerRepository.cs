using HomemadeFood.Api.Entities;
using HomemadeFood.Api.ReadModels;

namespace HomemadeFood.Api.Interfaces
{
    public interface IProducerRepository
    {
        Task AddAsync(
            ProducerProfile producerProfile);

        Task<bool> HasApplicationAsync(
            int userId);

        Task<ProducerProfile?> GetByUserIdAsync(
            int userId);

        Task<List<ProducerProfile>>
            GetPendingApplicationsAsync();

        Task<List<ProducerProfile>>
            GetApplicationsByStatusAsync(
                string verificationStatus);

        Task<ProducerProfile?>
            GetByIdWithUserAsync(
                int producerProfileId);

        Task<ProducerProfile?>
            GetApprovedByUserIdAsync(
                int userId);

        Task<List<ProducerStorefrontSummaryReadModel>>
            GetAvailableStorefrontsAsync(
                int? categoryId);

        Task<List<ProducerNearbyCandidateReadModel>>
            GetNearbyCandidatesAsync();

        Task<List<ProducerDiscoverCandidateReadModel>>
            GetDiscoverCandidatesAsync(
                int? categoryId,
                string? search,
                double minimumLatitude,
                double maximumLatitude,
                double minimumLongitude,
                double maximumLongitude,
                DateTime fromUtc);
        Task<List<ProducerDiscoverCandidateReadModel>>
           GetCityCandidatesAsync(
               string city,
               DateTime fromUtc);



        Task<List<ProducerPopularityCandidateReadModel>>
            GetPopularityCandidatesAsync(
                DateTime fromUtc);

        Task<ProducerStorefrontMenuReadModel?>
            GetAvailableStorefrontMenuAsync(
                int producerProfileId);

        Task SaveChangesAsync();
    }
}