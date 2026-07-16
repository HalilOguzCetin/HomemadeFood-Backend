using HomemadeFood.Api.DTOs.Order;
using HomemadeFood.Api.Interfaces;
using OrderEntity = HomemadeFood.Api.Entities.Order;
using OrderItemEntity = HomemadeFood.Api.Entities.OrderItem;
using HomemadeFood.Api.Constants;

namespace HomemadeFood.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IProducerCapacityService
            _producerCapacityService;

        public OrderService(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IAddressRepository addressRepository,
    IProducerCapacityService producerCapacityService)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _addressRepository = addressRepository;
            _producerCapacityService =
                producerCapacityService;
        }

        public async Task<OrderResponse?> CreateOrderAsync(
            int customerId,
            CreateOrderRequest request)
        {
            // Adres gerçekten giriş yapan müşteriye ait mi?
            var address =
                await _addressRepository
                    .GetByIdAndUserIdAsync(
                        request.AddressId,
                        customerId);

            if (address == null)
            {
                return null;
            }

            // Sepeti; yemek, kategori ve üretici
            // ayrıntılarıyla birlikte getir.
            var cart =
                await _cartRepository
                    .GetForOrderCreationAsync(customerId);

            if (cart == null || cart.Items.Count == 0)
            {
                return null;
            }

            var producer = cart.ProducerProfile;

            // Üretici hâlâ onaylı ve sipariş almaya açık mı?
            if (!producer.IsApproved ||
                !producer.IsAvailable ||
                producer.VerificationStatus !=
    ProducerVerificationStatuses.Approved)
            {
                return null;
            }

            // Sepetteki bütün yemekleri tekrar doğrula.
            var hasInvalidItem =
                cart.Items.Any(item =>
                    item.Quantity <= 0 ||
                    !item.Food.IsAvailable ||
                    !item.Food.Category.IsActive ||
                    item.Food.ProducerProfileId !=
                        cart.ProducerProfileId);

            if (hasInvalidItem)
            {
                return null;
            }

            var totalQuantity =
                cart.Items.Sum(item => item.Quantity);

            // Üreticinin kalan günlük kapasitesi yeterli mi?
            var capacityReserved =
     _producerCapacityService.TryReserve(
         producer,
         totalQuantity);

            if (!capacityReserved)
            {
                return null;
            }

            // Fiyat tamamen veritabanındaki güncel
            // yemek fiyatlarından hesaplanır.
            var totalPrice =
                cart.Items.Sum(item =>
                    item.Food.Price * item.Quantity);

            var now = DateTime.UtcNow;

            var order = new OrderEntity
            {
                CustomerId = customerId,

                ProducerProfileId = producer.Id,
                ProducerProfile = producer,

                DeliveryAddressTitle =
                    address.Title,

                DeliveryAddress =
                    address.FullAddress,

                DeliveryLatitude =
                    address.Latitude,

                DeliveryLongitude =
                    address.Longitude,

                PaymentMethod =
                    request.PaymentMethod.Trim(),

                CustomerNote =
                    request.CustomerNote?.Trim()
                    ?? string.Empty,

                TotalPrice = totalPrice,

                Status = OrderStatuses.Pending,

                SuitabilityScore = 0,

                CreatedAt = now,
                StatusUpdatedAt = now
            };

            foreach (var cartItem in cart.Items)
            {
                var unitPrice =
                    cartItem.Food.Price;

                order.OrderItems.Add(
                    new OrderItemEntity
                    {
                        FoodId =
                            cartItem.FoodId,

                        FoodName =
                            cartItem.Food.Name,

                        Quantity =
                            cartItem.Quantity,

                        UnitPrice =
                            unitPrice,

                        TotalPrice =
                            unitPrice *
                            cartItem.Quantity
                    });
            }

            // Sipariş miktarı kadar kapasite azalt.
            

            // Siparişi ekle.
            await _orderRepository.AddAsync(order);

            // Sipariş oluştuğu için sepeti kaldır.
            _cartRepository.Remove(cart);

            /*
             * Repository'ler aynı scoped AppDbContext örneğini
             * kullandığı için aşağıdaki tek SaveChangesAsync:
             *
             * - Order ekleme
             * - OrderItem ekleme
             * - Kapasite azaltma
             * - Sepeti ve CartItem kayıtlarını silme
             *
             * işlemlerini birlikte kaydeder.
             */
            await _orderRepository.SaveChangesAsync();

            return MapToResponse(order);
        }

        public async Task<List<OrderResponse>>
            GetMyOrdersAsync(int customerId)
        {
            var orders =
                await _orderRepository
                    .GetByCustomerIdAsync(customerId);

            return orders
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<OrderResponse?>
            GetMyOrderByIdAsync(
                int customerId,
                int orderId)
        {
            var order =
                await _orderRepository
                    .GetByIdAndCustomerIdAsync(
                        orderId,
                        customerId);

            if (order == null)
            {
                return null;
            }

            return MapToResponse(order);
        }
        public async Task<OrderResponse?> CancelOrderAsync(
    int customerId,
    int orderId)
        {
            var order =
                await _orderRepository
                    .GetTrackedByIdAndCustomerIdAsync(
                        orderId,
                        customerId);

            if (order == null)
            {
                return null;
            }

            // Müşteri yalnızca üretici henüz kabul etmeden
            // Pending durumundaki siparişi iptal edebilir.
            if (!string.Equals(
         order.Status,
         OrderStatuses.Pending,
         StringComparison.Ordinal))
            {
                return null;
            }

            var totalQuantity =
                order.OrderItems.Sum(x => x.Quantity);

            // Sipariş oluşturulurken azaltılan kapasiteyi geri ver.
            _producerCapacityService.RestoreForOrder(
    order.ProducerProfile,
    order.CreatedAt,
    totalQuantity);

            order.Status = OrderStatuses.Cancelled;
            order.StatusUpdatedAt = DateTime.UtcNow;

            await _orderRepository.SaveChangesAsync();

            return MapToResponse(order);
        }

        private static OrderResponse MapToResponse(
            OrderEntity order)
        {
            return new OrderResponse
            {
                OrderId = order.Id,

                ProducerProfileId =
                    order.ProducerProfileId,

                BusinessName =
                    order.ProducerProfile.BusinessName,

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