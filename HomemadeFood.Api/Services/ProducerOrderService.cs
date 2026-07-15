using HomemadeFood.Api.DTOs.Order;
using HomemadeFood.Api.DTOs.ProducerOrder;
using HomemadeFood.Api.Interfaces;
using OrderEntity = HomemadeFood.Api.Entities.Order;

namespace HomemadeFood.Api.Services
{
    public class ProducerOrderService : IProducerOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProducerRepository _producerRepository;

        public ProducerOrderService(
            IOrderRepository orderRepository,
            IProducerRepository producerRepository)
        {
            _orderRepository = orderRepository;
            _producerRepository = producerRepository;
        }

        public async Task<List<ProducerOrderResponse>>
            GetMyOrdersAsync(int producerUserId)
        {
            var producerProfile =
                await _producerRepository
                    .GetApprovedByUserIdAsync(producerUserId);

            if (producerProfile == null)
            {
                return new List<ProducerOrderResponse>();
            }

            var orders =
                await _orderRepository
                    .GetByProducerProfileIdAsync(
                        producerProfile.Id);

            return orders
                .Select(MapToResponse)
                .ToList();
        }

        public Task<ProducerOrderResponse?> AcceptOrderAsync(
            int producerUserId,
            int orderId)
        {
            return ChangeStatusAsync(
                producerUserId,
                orderId,
                expectedStatus: "Pending",
                newStatus: "Accepted",
                restoreCapacity: false);
        }

        public Task<ProducerOrderResponse?> RejectOrderAsync(
            int producerUserId,
            int orderId)
        {
            return ChangeStatusAsync(
                producerUserId,
                orderId,
                expectedStatus: "Pending",
                newStatus: "Rejected",
                restoreCapacity: true);
        }

        public Task<ProducerOrderResponse?> StartPreparingAsync(
            int producerUserId,
            int orderId)
        {
            return ChangeStatusAsync(
                producerUserId,
                orderId,
                expectedStatus: "Accepted",
                newStatus: "Preparing",
                restoreCapacity: false);
        }

        public Task<ProducerOrderResponse?> MarkReadyAsync(
            int producerUserId,
            int orderId)
        {
            return ChangeStatusAsync(
                producerUserId,
                orderId,
                expectedStatus: "Preparing",
                newStatus: "Ready",
                restoreCapacity: false);
        }

        public Task<ProducerOrderResponse?>
            MarkOutForDeliveryAsync(
                int producerUserId,
                int orderId)
        {
            return ChangeStatusAsync(
                producerUserId,
                orderId,
                expectedStatus: "Ready",
                newStatus: "OutForDelivery",
                restoreCapacity: false);
        }

        public Task<ProducerOrderResponse?> MarkDeliveredAsync(
            int producerUserId,
            int orderId)
        {
            return ChangeStatusAsync(
                producerUserId,
                orderId,
                expectedStatus: "OutForDelivery",
                newStatus: "Delivered",
                restoreCapacity: false);
        }

        private async Task<ProducerOrderResponse?>
            ChangeStatusAsync(
                int producerUserId,
                int orderId,
                string expectedStatus,
                string newStatus,
                bool restoreCapacity)
        {
            var producerProfile =
                await _producerRepository
                    .GetApprovedByUserIdAsync(producerUserId);

            if (producerProfile == null)
            {
                return null;
            }

            var order =
                await _orderRepository
                    .GetTrackedByIdAndProducerProfileIdAsync(
                        orderId,
                        producerProfile.Id);

            if (order == null)
            {
                return null;
            }

            // Durumların yanlış sırayla değiştirilmesini engeller.
            if (!string.Equals(
                    order.Status,
                    expectedStatus,
                    StringComparison.Ordinal))
            {
                return null;
            }

            if (restoreCapacity)
            {
                var totalQuantity =
                    order.OrderItems.Sum(x => x.Quantity);

                // Reddedilen siparişte daha önce düşürülen
                // kapasite üreticiye geri verilir.
                order.ProducerProfile.RemainingCapacity =
                    Math.Min(
                        order.ProducerProfile.DailyCapacity,
                        order.ProducerProfile.RemainingCapacity
                        + totalQuantity);
            }

            order.Status = newStatus;
            order.StatusUpdatedAt = DateTime.UtcNow;

            await _orderRepository.SaveChangesAsync();

            return MapToResponse(order);
        }

        private static ProducerOrderResponse MapToResponse(
            OrderEntity order)
        {
            return new ProducerOrderResponse
            {
                OrderId = order.Id,

                CustomerFullName =
                    order.Customer.FullName,

                CustomerPhone =
                    order.Customer.Phone,

                DeliveryAddressTitle =
                    order.DeliveryAddressTitle,

                DeliveryAddress =
                    order.DeliveryAddress,

                DeliveryLatitude =
                    order.DeliveryLatitude,

                DeliveryLongitude =
                    order.DeliveryLongitude,

                PaymentMethod =
                    order.PaymentMethod,

                CustomerNote =
                    order.CustomerNote,

                TotalQuantity =
                    order.OrderItems.Sum(x => x.Quantity),

                TotalPrice =
                    order.TotalPrice,

                Status =
                    order.Status,

                CreatedAt =
                    order.CreatedAt,

                StatusUpdatedAt =
                    order.StatusUpdatedAt,

                Items = order.OrderItems
                    .Select(item =>
                        new OrderItemResponse
                        {
                            OrderItemId =
                                item.Id,

                            FoodId =
                                item.FoodId,

                            FoodName =
                                item.FoodName,

                            Quantity =
                                item.Quantity,

                            UnitPrice =
                                item.UnitPrice,

                            TotalPrice =
                                item.TotalPrice
                        })
                    .ToList()
            };
        }
    }
}