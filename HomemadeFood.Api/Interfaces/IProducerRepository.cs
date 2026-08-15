using HomemadeFood.Api.Entities;
using HomemadeFood.Api.ReadModels;

namespace HomemadeFood.Api.Interfaces
{
    public interface IProducerRepository
    {
        Task AddAsync(ProducerProfile producerProfile);

        Task<bool> HasApplicationAsync(int userId);
        Task<ProducerProfile?> GetByUserIdAsync(
    int userId);

        Task<List<ProducerProfile>> GetPendingApplicationsAsync();
        Task<List<ProducerProfile>>
    GetApplicationsByStatusAsync(
        string verificationStatus);

        Task<ProducerProfile?> GetByIdWithUserAsync(int producerProfileId);
        Task<ProducerProfile?> GetApprovedByUserIdAsync(int userId);

        Task<List<ProducerStorefrontSummaryReadModel>>
            GetAvailableStorefrontsAsync(
                int? categoryId);

        Task<ProducerStorefrontMenuReadModel?>
            GetAvailableStorefrontMenuAsync(
                int producerProfileId);

        Task SaveChangesAsync();
    }
}