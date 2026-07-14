using HomemadeFood.Api.DTOs.Order;

namespace HomemadeFood.Api.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponse?> CreateOrderAsync(
            int customerId,
            CreateOrderRequest request);

        Task<List<OrderResponse>> GetMyOrdersAsync(
            int customerId);

        Task<OrderResponse?> GetMyOrderByIdAsync(
            int customerId,
            int orderId);
    }
}