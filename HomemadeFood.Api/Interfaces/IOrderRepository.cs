using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface IOrderRepository
    {
        Task AddAsync(
            Order order);

        Task<List<Order>> GetByCustomerIdAsync(
            int customerId);

        Task<Order?> GetByIdAndCustomerIdAsync(
            int orderId,
            int customerId);

        Task<Order?>
            GetTrackedByIdAndCustomerIdAsync(
                int orderId,
                int customerId);

        Task<List<Order>>
            GetByProducerProfileIdAsync(
                int producerProfileId);

        Task<Order?>
            GetTrackedByIdAndProducerProfileIdAsync(
                int orderId,
                int producerProfileId);

        Task<List<Order>> GetForAdminAsync(
            string? status,
            int? customerId,
            int? producerProfileId,
            string? search,
            DateTime? dateFrom,
            DateTime? dateToExclusive);

        Task<Order?> GetByIdForAdminAsync(
            int orderId);

        Task SaveChangesAsync();
    }
}