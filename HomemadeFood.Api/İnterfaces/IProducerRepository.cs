using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface IProducerRepository
    {
        Task AddAsync(ProducerProfile producerProfile);

        Task<bool> HasApplicationAsync(int userId);

        Task<List<ProducerProfile>> GetPendingApplicationsAsync();

        Task<ProducerProfile?> GetByIdWithUserAsync(int producerProfileId);

        Task SaveChangesAsync();
    }
}