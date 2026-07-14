using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);

        Task<List<Order>> GetByCustomerIdAsync(
            int customerId);

        Task<Order?> GetByIdAndCustomerIdAsync(
            int orderId,
            int customerId);

        Task SaveChangesAsync();
    }
}