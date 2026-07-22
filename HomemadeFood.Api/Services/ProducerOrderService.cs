using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Order;
using HomemadeFood.Api.DTOs.ProducerOrder;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using OrderEntity = HomemadeFood.Api.Entities.Order;

namespace HomemadeFood.Api.Services
{
    public class ProducerOrderService : IProducerOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProducerRepository _producerRepository;

        private readonly IProducerCapacityService
            _producerCapacityService;

        private readonly IAppClock _appClock;

        public ProducerOrderService(
            IOrderRepository orderRepository,
            IProducerRepository producerRepository,
            IProducerCapacityService producerCapacityService,
            IAppClock appClock)
        {
            _orderRepository = orderRepository;
            _producerRepository = producerRepository;

            _producerCapacityService =
                producerCapacityService;

            _appClock = appClock;
        }

        public async Task<List<ProducerOrderResponse>>
            GetMyOrdersAsync(
                int producerUserId)
        {
            var producerProfile =
                await _producerRepository
                    .GetApprovedByUserIdAsync(
                        producerUserId);

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

        public Task<ProducerOrderResponse?>
            AcceptOrderAsync(
                int producerUserId,
                int orderId)
        {
            return ChangeStatusAsync(
                producerUserId,
                orderId,
                expectedStatus:
                    OrderStatuses.Pending,
                newStatus:
                    OrderStatuses.Accepted,
                restoreCapacity: false);
        }

        public Task<ProducerOrderResponse?>
            RejectOrderAsync(
                int producerUserId,
                int orderId)
        {
            return ChangeStatusAsync(
                producerUserId,
                orderId,
                expectedStatus:
                    OrderStatuses.Pending,
                newStatus:
                    OrderStatuses.Rejected,
                restoreCapacity: true);
        }

        public Task<ProducerOrderResponse?>
            StartPreparingAsync(
                int producerUserId,
                int orderId)
        {
            return ChangeStatusAsync(
                producerUserId,
                orderId,
                expectedStatus:
                    OrderStatuses.Accepted,
                newStatus:
                    OrderStatuses.Preparing,
                restoreCapacity: false);
        }

        public Task<ProducerOrderResponse?>
            MarkReadyAsync(
                int producerUserId,
                int orderId)
        {
            return ChangeStatusAsync(
                producerUserId,
                orderId,
                expectedStatus:
                    OrderStatuses.Preparing,
                newStatus:
                    OrderStatuses.Ready,
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
                expectedStatus:
                    OrderStatuses.Ready,
                newStatus:
                    OrderStatuses.OutForDelivery,
                restoreCapacity: false);
        }

        public Task<ProducerOrderResponse?>
            MarkDeliveredAsync(
                int producerUserId,
                int orderId)
        {
            return ChangeStatusAsync(
                producerUserId,
                orderId,
                expectedStatus:
                    OrderStatuses.OutForDelivery,
                newStatus:
                    OrderStatuses.Delivered,
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
                    .GetApprovedByUserIdAsync(
                        producerUserId);

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

            // Sipariş durumlarının yanlış sırayla
            // değiştirilmesini engeller.
            if (!string.Equals(
                    order.Status,
                    expectedStatus,
                    StringComparison.Ordinal))
            {
                return null;
            }

            /*
             * Sipariş reddediliyorsa, sipariş oluşturulurken
             * ayrılan günlük kapasite geri verilir.
             *
             * Kapasite değişikliği ve sipariş durum değişikliği
             * aynı SaveChangesAsync içinde kaydedilir.
             */
            if (restoreCapacity)
            {
                var totalQuantity =
                    order.OrderItems.Sum(
                        item => item.Quantity);

                _producerCapacityService
                    .RestoreForOrder(
                        order.ProducerProfile,
                        order.CreatedAt,
                        totalQuantity);
            }

            order.Status =
                newStatus;

            order.StatusUpdatedAt =
                _appClock.UtcNow;

            /*
             * StatusVersion bir concurrency token olduğu için
             * SQL güncellemesinde eski sürüm değeri kontrol edilir.
             *
             * İki istek aynı siparişi aynı anda değiştirmeye
             * çalışırsa yalnızca ilk işlem başarılı olur.
             */
            order.StatusVersion++;

            try
            {
                await _orderRepository
                    .SaveChangesAsync();

                return MapToResponse(order);
            }
            catch (DbUpdateConcurrencyException)
            {
                /*
                 * Sipariş durumu başka bir işlem tarafından
                 * bizden önce değiştirilmiştir.
                 *
                 * SaveChanges başarısız olduğu için:
                 *
                 * - İkinci durum değişikliği kaydedilmez
                 * - Kapasite değişikliği kaydedilmez
                 * - İşlem güvenli biçimde başarısız döner
                 */
                return null;
            }
        }

        private static ProducerOrderResponse MapToResponse(
            OrderEntity order)
        {
            return new ProducerOrderResponse
            {
                OrderId =
                    order.Id,

                RecommendationSearchId =
                    order.RecommendationSearchId,

                SuitabilityScore =
                    order.SuitabilityScore,

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
                    order.OrderItems.Sum(
                        item => item.Quantity),

                TotalPrice =
                    order.TotalPrice,

                Status =
                    order.Status,

                CreatedAt =
                    order.CreatedAt,

                StatusUpdatedAt =
                    order.StatusUpdatedAt,

                Items =
                    order.OrderItems
                        .Select(
                            item =>
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