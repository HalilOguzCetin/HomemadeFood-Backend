using HomemadeFood.Api.Constants;
using HomemadeFood.Api.Data;
using HomemadeFood.Api.DTOs.Order;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using OrderEntity = HomemadeFood.Api.Entities.Order;
using OrderItemEntity = HomemadeFood.Api.Entities.OrderItem;

namespace HomemadeFood.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IAddressRepository _addressRepository;

        private readonly IProducerCapacityService
            _producerCapacityService;

        private readonly IAppClock _appClock;
        private readonly AppDbContext _dbContext;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IAddressRepository addressRepository,
            IProducerCapacityService producerCapacityService,
            IAppClock appClock,
            AppDbContext dbContext)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _addressRepository = addressRepository;

            _producerCapacityService =
                producerCapacityService;

            _appClock = appClock;
            _dbContext = dbContext;
        }

        public async Task<OrderResponse?> CreateOrderAsync(
            int customerId,
            CreateOrderRequest request)
        {
            // Gönderilen adres gerçekten giriş yapan
            // müşteriye ait mi?
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
                    .GetForOrderCreationAsync(
                        customerId);

            if (cart == null ||
                cart.Items.Count == 0)
            {
                return null;
            }

            var producer =
                cart.ProducerProfile;

            // Üretici hâlâ onaylı ve
            // sipariş almaya açık mı?
            if (!producer.IsApproved ||
                !producer.IsAvailable ||
                producer.VerificationStatus !=
                ProducerVerificationStatuses.Approved)
            {
                return null;
            }

            // Sepetteki bütün yemekleri
            // sipariş öncesinde tekrar doğrula.
            var hasInvalidItem =
                cart.Items.Any(
                    item =>
                        item.Quantity <= 0 ||

                        !item.Food.IsAvailable ||

                        !item.Food.Category.IsActive ||

                        item.Food.ProducerProfileId !=
                        cart.ProducerProfileId);

            if (hasInvalidItem)
            {
                return null;
            }

            /*
             * Normal yemek listesinden oluşturulan siparişlerde:
             *
             * RecommendationSearchId = null
             * SuitabilityScore = 0
             *
             * Öneri ekranından oluşturulan siparişlerdeyse
             * aşağıdaki değerler doğrulanarak doldurulur.
             */
            int? recommendationSearchId = null;

            decimal suitabilityScore = 0;

            if (cart.RecommendationSearchId.HasValue)
            {
                var searchRecord =
                    await _dbContext
                        .RecommendationSearches
                        .AsNoTracking()
                        .FirstOrDefaultAsync(
                            x =>
                                x.Id ==
                                cart.RecommendationSearchId.Value &&

                                x.CustomerUserId ==
                                customerId);

                // Öneri araması bulunamadıysa veya
                // müşteri henüz bir öneri seçmediyse
                // sipariş oluşturulmaz.
                if (searchRecord == null ||
                    !searchRecord.SelectedFoodId.HasValue ||
                    !searchRecord
                        .SelectedProducerProfileId
                        .HasValue ||
                    !searchRecord.SelectedAtUtc.HasValue)
                {
                    return null;
                }

                // Öneride seçilen üretici ile
                // sepetteki üretici aynı olmalıdır.
                if (searchRecord
                        .SelectedProducerProfileId
                        .Value !=
                    cart.ProducerProfileId)
                {
                    return null;
                }

                // Öneride seçilen yemek gerçekten
                // sepette bulunmalıdır.
                var selectedFoodIsInCart =
                    cart.Items.Any(
                        item =>
                            item.FoodId ==
                            searchRecord
                                .SelectedFoodId
                                .Value);

                if (!selectedFoodIsInCart)
                {
                    return null;
                }

                // Seçimin, arama sırasında kullanıcıya
                // gösterilmiş gerçek bir aday olduğu doğrulanır.
                var selectedCandidate =
                    await _dbContext
                        .RecommendationCandidates
                        .AsNoTracking()
                        .FirstOrDefaultAsync(
                            candidate =>
                                candidate
                                    .RecommendationSearchId ==
                                searchRecord.Id &&

                                candidate.FoodId ==
                                searchRecord
                                    .SelectedFoodId
                                    .Value &&

                                candidate.ProducerProfileId ==
                                searchRecord
                                    .SelectedProducerProfileId
                                    .Value);

                if (selectedCandidate == null)
                {
                    return null;
                }

                recommendationSearchId =
                    searchRecord.Id;

                suitabilityScore =
                    Math.Round(
                        (decimal)
                        selectedCandidate.TotalScore,
                        2);
            }

            // Metot içinde yalnızca bir kez tanımlanır.
            var totalQuantity =
                cart.Items.Sum(
                    item => item.Quantity);

            // Üreticinin kalan günlük kapasitesi
            // toplam sipariş miktarı için yeterli mi?
            var capacityReserved =
                _producerCapacityService.TryReserve(
                    producer,
                    totalQuantity);

            if (!capacityReserved)
            {
                return null;
            }

            // Toplam fiyat, istemciden gelen bir değerden
            // değil veritabanındaki güncel fiyatlardan hesaplanır.
            var totalPrice =
                cart.Items.Sum(
                    item =>
                        item.Food.Price *
                        item.Quantity);

            var now =
                _appClock.UtcNow;

            var order =
                new OrderEntity
                {
                    CustomerId =
                        customerId,

                    ProducerProfileId =
                        producer.Id,

                    ProducerProfile =
                        producer,

                    RecommendationSearchId =
                        recommendationSearchId,

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

                    TotalPrice =
                        totalPrice,

                    Status =
                        OrderStatuses.Pending,

                    SuitabilityScore =
                        suitabilityScore,

                    CreatedAt =
                        now,

                    StatusUpdatedAt =
                        now
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

            // Siparişi aynı DbContext içerisine ekle.
            await _orderRepository
                .AddAsync(order);

            // Sipariş oluşturulduğu için sepeti kaldır.
            _cartRepository.Remove(cart);

            /*
             * Repository sınıfları ve AppDbContext aynı scoped
             * DbContext örneğini kullandığı için tek SaveChanges:
             *
             * - Order kaydını ekler
             * - OrderItem kayıtlarını ekler
             * - Üretici kapasitesini azaltır
             * - Öneri bağlantısını siparişe kaydeder
             * - Sepeti ve CartItem kayıtlarını siler
             *
             * İşlemlerden biri başarısız olursa değişikliklerin
             * hiçbiri veritabanına kalıcı olarak yazılmaz.
             */
            try
            {
                await _orderRepository
                    .SaveChangesAsync();

                return MapToResponse(order);
            }
            catch (DbUpdateConcurrencyException)
            {
                /*
                 * Başka bir sipariş aynı üreticinin
                 * kapasitesini bizden önce değiştirmiştir.
                 *
                 * SaveChanges başarısız olduğu için:
                 *
                 * - Sipariş kaydedilmez
                 * - Kapasite azaltılmaz
                 * - Sepet silinmez
                 */
                return null;
            }
        }

        public async Task<List<OrderResponse>>
            GetMyOrdersAsync(
                int customerId)
        {
            var orders =
                await _orderRepository
                    .GetByCustomerIdAsync(
                        customerId);

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

        public async Task<OrderResponse?>
            CancelOrderAsync(
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

            // Müşteri yalnızca üretici henüz
            // kabul etmeden Pending siparişi iptal edebilir.
            if (!string.Equals(
                    order.Status,
                    OrderStatuses.Pending,
                    StringComparison.Ordinal))
            {
                return null;
            }

            var totalQuantity =
                order.OrderItems.Sum(
                    item => item.Quantity);

            // Sipariş oluşturulurken azaltılan
            // kapasiteyi uygun durumda geri ver.
            _producerCapacityService
                .RestoreForOrder(
                    order.ProducerProfile,
                    order.CreatedAt,
                    totalQuantity);

            order.Status =
                OrderStatuses.Cancelled;

            order.StatusUpdatedAt =
                _appClock.UtcNow;

            try
            {
                await _orderRepository
                    .SaveChangesAsync();

                return MapToResponse(order);
            }
            catch (DbUpdateConcurrencyException)
            {
                /*
                 * Kapasite başka bir işlem tarafından aynı anda
                 * değiştirildiyse iptal işlemi güvenli biçimde
                 * başarısız olur.
                 */
                return null;
            }
        }

        private static OrderResponse MapToResponse(
            OrderEntity order)
        {
            return new OrderResponse
            {
                OrderId =
                    order.Id,

                ProducerProfileId =
                    order.ProducerProfileId,

                BusinessName =
                    order.ProducerProfile.BusinessName,
                RecommendationSearchId =
    order.RecommendationSearchId,

                SuitabilityScore =
    order.SuitabilityScore,

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