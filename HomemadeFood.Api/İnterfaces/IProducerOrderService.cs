using HomemadeFood.Api.DTOs.ProducerOrder;

namespace HomemadeFood.Api.Interfaces
{
    public interface IProducerOrderService
    {
        Task<List<ProducerOrderResponse>>
            GetMyOrdersAsync(int producerUserId);

        Task<ProducerOrderResponse?> AcceptOrderAsync(
            int producerUserId,
            int orderId);

        Task<ProducerOrderResponse?> RejectOrderAsync(
            int producerUserId,
            int orderId);

        Task<ProducerOrderResponse?> StartPreparingAsync(
            int producerUserId,
            int orderId);

        Task<ProducerOrderResponse?> MarkReadyAsync(
            int producerUserId,
            int orderId);

        Task<ProducerOrderResponse?> MarkOutForDeliveryAsync(
            int producerUserId,
            int orderId);

        Task<ProducerOrderResponse?> MarkDeliveredAsync(
            int producerUserId,
            int orderId);
    }
}