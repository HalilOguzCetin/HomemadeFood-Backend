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

        private readonly IProducerAvailabilityService
            _producerAvailabilityService;

        private readonly AppDbContext _dbContext;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IAddressRepository addressRepository,
            IProducerCapacityService producerCapacityService,
            IAppClock appClock,
            IProducerAvailabilityService
                producerAvailabilityService,
            AppDbContext dbContext)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _addressRepository = addressRepository;

            _producerCapacityService =
                producerCapacityService;

            _appClock = appClock;

            _producerAvailabilityService =
                producerAvailabilityService;

            _dbContext = dbContext;
        }

        public async Task<bool>
            IsPhoneVerifiedForOrderAsync(
                int customerId)
        {
            if (customerId <= 0)
            {
                return false;
            }

            return await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(user =>
                    user.Id == customerId &&
                    user.IsActive &&
                    user.Role == UserRoles.Customer &&
                    user.IsPhoneVerified &&
                    user.PhoneVerifiedAt.HasValue &&
                    user.NormalizedPhone != null &&
                    user.NormalizedPhone != "");
        }

        public async Task<OrderResponse?> CreateOrderAsync(
            int customerId,
            CreateOrderRequest request)
        {
            var hasVerifiedPhone =
                await IsPhoneVerifiedForOrderAsync(
                    customerId);

            if (!hasVerifiedPhone)
            {
                return null;
            }

            var address =
                await _addressRepository
                    .GetByIdAndUserIdAsync(
                        request.AddressId,
                        customerId);

            if (address == null)
            {
                return null;
            }

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

            if (!producer.IsApproved ||
                !producer.IsAvailable ||
                producer.VerificationStatus !=
                ProducerVerificationStatuses.Approved)
            {
                return null;
            }

            var isProducerCurrentlyOpen =
                await _producerAvailabilityService
                    .IsProducerCurrentlyOpenAsync(
                        cart.ProducerProfileId);

            if (!isProducerCurrentlyOpen)
            {
                return null;
            }

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

                if (searchRecord == null ||
                    !searchRecord.SelectedFoodId.HasValue ||
                    !searchRecord
                        .SelectedProducerProfileId
                        .HasValue ||
                    !searchRecord.SelectedAtUtc.HasValue)
                {
                    return null;
                }

                if (searchRecord
                        .SelectedProducerProfileId
                        .Value !=
                    cart.ProducerProfileId)
                {
                    return null;
                }

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

            var totalQuantity =
                cart.Items.Sum(
                    item => item.Quantity);

            var capacityReserved =
                _producerCapacityService.TryReserve(
                    producer,
                    totalQuantity);

            if (!capacityReserved)
            {
                return null;
            }

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

            await _orderRepository
                .AddAsync(order);

            _cartRepository.Remove(cart);

            try
            {
                await _orderRepository
                    .SaveChangesAsync();

                return MapToResponse(order);
            }
            catch (DbUpdateConcurrencyException)
            {
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

            _producerCapacityService
                .RestoreForOrder(
                    order.ProducerProfile,
                    order.CreatedAt,
                    totalQuantity);

            order.Status =
                OrderStatuses.Cancelled;

            order.StatusUpdatedAt =
                _appClock.UtcNow;

            order.StatusVersion++;

            try
            {
                await _orderRepository
                    .SaveChangesAsync();

                return MapToResponse(order);
            }
            catch (DbUpdateConcurrencyException)
            {
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