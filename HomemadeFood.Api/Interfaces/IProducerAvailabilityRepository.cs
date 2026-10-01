using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface IProducerAvailabilityRepository
    {
        Task<ProducerProfile?>
            GetApprovedByUserIdWithBusinessHoursAsync(
                int userId);

        Task<List<ProducerProfile>>
            GetByIdsWithBusinessHoursAsync(
                IEnumerable<int>
                    producerProfileIds);

        void RemoveBusinessHours(
            IEnumerable<ProducerBusinessHour>
                businessHours);

        Task AddBusinessHoursAsync(
            IEnumerable<ProducerBusinessHour>
                businessHours);

        Task SaveChangesAsync();
    }
}
