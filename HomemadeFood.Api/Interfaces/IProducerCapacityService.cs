using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface IProducerCapacityService
    {
        void EnsureCurrentDay(
            ProducerProfile producerProfile);

        bool TryReserve(
            ProducerProfile producerProfile,
            int quantity);

        void RestoreForOrder(
            ProducerProfile producerProfile,
            DateTime orderCreatedAt,
            int quantity);
    }
}