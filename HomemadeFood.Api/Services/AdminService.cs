using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Admin;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public class AdminService : IAdminService
    {
        private readonly IProducerRepository
            _producerRepository;

        private readonly IUserRepository
            _userRepository;
        private readonly IOrderRepository
    _orderRepository;

        private readonly IAppClock
            _appClock;

        public AdminService(
     IProducerRepository producerRepository,
     IUserRepository userRepository,
     IOrderRepository orderRepository,
     IAppClock appClock)
        {
            _producerRepository =
                producerRepository;

            _userRepository =
                userRepository;
            _orderRepository =
    orderRepository;

            _appClock =
                appClock;
        }

        public async Task<
            List<AdminProducerApplicationResponse>>
            GetProducerApplicationsAsync(
                string verificationStatus)
        {
            var normalizedStatus =
                NormalizeVerificationStatus(
                    verificationStatus);

            if (normalizedStatus == null)
            {
                return new List<
                    AdminProducerApplicationResponse>();
            }

            var applications =
                await _producerRepository
                    .GetApplicationsByStatusAsync(
                        normalizedStatus);

            return applications
                .Select(MapProducerApplication)
                .ToList();
        }

        public async Task<bool> ApproveProducerAsync(
            int producerProfileId,
            int adminUserId)
        {
            var producerProfile =
                await _producerRepository
                    .GetByIdWithUserAsync(
                        producerProfileId);

            if (producerProfile == null)
            {
                return false;
            }

            if (!string.Equals(
                    producerProfile
                        .VerificationStatus,

                    ProducerVerificationStatuses
                        .Pending,

                    StringComparison.Ordinal))
            {
                return false;
            }

            producerProfile.IsApproved =
                true;

            producerProfile.IsAvailable =
                true;

            producerProfile.VerificationStatus =
                ProducerVerificationStatuses
                    .Approved;

            producerProfile.ApprovedAt =
                _appClock.UtcNow;

            producerProfile.ApprovedByAdminId =
                adminUserId;

            producerProfile.RejectedAt =
                null;

            producerProfile.RejectedByAdminId =
                null;

            producerProfile.RejectionReason =
                null;

            producerProfile.User.Role =
                UserRoles.Producer;

            await _producerRepository
                .SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectProducerAsync(
            int producerProfileId,
            int adminUserId,
            string rejectionReason)
        {
            var producerProfile =
                await _producerRepository
                    .GetByIdWithUserAsync(
                        producerProfileId);

            if (producerProfile == null)
            {
                return false;
            }

            if (!string.Equals(
                    producerProfile
                        .VerificationStatus,

                    ProducerVerificationStatuses
                        .Pending,

                    StringComparison.Ordinal))
            {
                return false;
            }

            var normalizedReason =
                rejectionReason.Trim();

            if (normalizedReason.Length < 10 ||
                normalizedReason.Length > 500)
            {
                return false;
            }

            producerProfile.IsApproved =
                false;

            producerProfile.IsAvailable =
                false;

            producerProfile.VerificationStatus =
                ProducerVerificationStatuses
                    .Rejected;

            producerProfile.RejectedAt =
                _appClock.UtcNow;

            producerProfile.RejectedByAdminId =
                adminUserId;

            producerProfile.RejectionReason =
                normalizedReason;

            producerProfile.ApprovedAt =
                null;

            producerProfile.ApprovedByAdminId =
                null;

            producerProfile.User.Role =
                UserRoles.Customer;

            await _producerRepository
                .SaveChangesAsync();

            return true;
        }

        public async Task<
            List<AdminUserListItemResponse>>
            GetUsersAsync(
                string? role,
                bool? isActive,
                string? search)
        {
            string? normalizedRole =
                null;

            if (!string.IsNullOrWhiteSpace(role))
            {
                normalizedRole =
                    NormalizeUserRole(role);

                if (normalizedRole == null)
                {
                    return new List<
                        AdminUserListItemResponse>();
                }
            }

            var normalizedSearch =
                string.IsNullOrWhiteSpace(search)
                    ? null
                    : search.Trim();

            var users =
                await _userRepository
                    .GetUsersAsync(
                        normalizedRole,
                        isActive,
                        normalizedSearch);

            return users
                .Select(MapUserListItem)
                .ToList();
        }

        public async Task<AdminUserDetailResponse?>
            GetUserByIdAsync(
                int userId)
        {
            if (userId <= 0)
            {
                return null;
            }

            var user =
                await _userRepository
                    .GetByIdWithAdminDetailsAsync(
                        userId);

            if (user == null)
            {
                return null;
            }

            return MapUserDetail(user);
        }

        public async Task<bool> UpdateUserStatusAsync(
            int userId,
            bool isActive)
        {
            if (userId <= 0)
            {
                return false;
            }

            var user =
                await _userRepository
                    .GetByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            user.IsActive =
                isActive;

            /*
             * Producer hesabı pasifleştirilirse
             * yeni sipariş almaya devam etmemelidir.
             */
            if (!isActive &&
                user.ProducerProfile != null)
            {
                user.ProducerProfile.IsAvailable =
                    false;
            }

            /*
             * Hesap tekrar aktifleştirildiğinde
             * üretici otomatik olarak siparişe
             * açılmaz. Üretici bunu profilinden
             * kendisi açmalıdır.
             */
            await _userRepository
                .SaveChangesAsync();

            return true;
        }
        public async Task<
    List<AdminOrderListItemResponse>>
    GetOrdersAsync(
        string? status,
        int? customerId,
        int? producerProfileId,
        string? search,
        DateOnly? dateFrom,
        DateOnly? dateTo)
        {
            string? normalizedStatus =
                null;

            if (!string.IsNullOrWhiteSpace(status))
            {
                normalizedStatus =
                    NormalizeOrderStatus(status);

                if (normalizedStatus == null)
                {
                    return new List<
                        AdminOrderListItemResponse>();
                }
            }

            var normalizedSearch =
                string.IsNullOrWhiteSpace(search)
                    ? null
                    : search.Trim();

            DateTime? normalizedDateFrom =
                null;

            if (dateFrom.HasValue)
            {
                normalizedDateFrom =
                    DateTime.SpecifyKind(
                        dateFrom.Value.ToDateTime(
                            TimeOnly.MinValue),

                        DateTimeKind.Utc);
            }

            DateTime? normalizedDateToExclusive =
                null;

            if (dateTo.HasValue)
            {
                normalizedDateToExclusive =
                    DateTime.SpecifyKind(
                        dateTo.Value
                            .AddDays(1)
                            .ToDateTime(
                                TimeOnly.MinValue),

                        DateTimeKind.Utc);
            }

            var orders =
                await _orderRepository
                    .GetForAdminAsync(
                        normalizedStatus,
                        customerId,
                        producerProfileId,
                        normalizedSearch,
                        normalizedDateFrom,
                        normalizedDateToExclusive);

            return orders
                .Select(MapOrderListItem)
                .ToList();
        }

        public async Task<AdminOrderDetailResponse?>
            GetOrderByIdAsync(
                int orderId)
        {
            if (orderId <= 0)
            {
                return null;
            }

            var order =
                await _orderRepository
                    .GetByIdForAdminAsync(
                        orderId);

            if (order == null)
            {
                return null;
            }

            return MapOrderDetail(order);
        }
        private static AdminOrderListItemResponse
    MapOrderListItem(
        Order order)
        {
            return new AdminOrderListItemResponse
            {
                OrderId =
                    order.Id,

                CustomerId =
                    order.CustomerId,

                CustomerFullName =
                    order.Customer.FullName,

                CustomerEmail =
                    order.Customer.Email,

                ProducerProfileId =
                    order.ProducerProfileId,

                BusinessName =
                    order.ProducerProfile
                        .BusinessName,

                Status =
                    order.Status,

                PaymentMethod =
                    order.PaymentMethod,

                TotalPrice =
                    order.TotalPrice,

                ItemCount =
                    order.OrderItems.Count,

                TotalQuantity =
                    order.OrderItems
                        .Sum(item =>
                            item.Quantity),

                RecommendationSearchId =
                    order.RecommendationSearchId,

                SuitabilityScore =
                    order.SuitabilityScore,

                CreatedAt =
                    order.CreatedAt,

                StatusUpdatedAt =
                    order.StatusUpdatedAt
            };
        }

        private static AdminOrderDetailResponse
            MapOrderDetail(
                Order order)
        {
            return new AdminOrderDetailResponse
            {
                OrderId =
                    order.Id,

                CustomerId =
                    order.CustomerId,

                CustomerFullName =
                    order.Customer.FullName,

                CustomerEmail =
                    order.Customer.Email,

                CustomerPhone =
                    order.Customer.Phone,

                ProducerProfileId =
                    order.ProducerProfileId,

                BusinessName =
                    order.ProducerProfile
                        .BusinessName,

                Status =
                    order.Status,

                StatusVersion =
                    order.StatusVersion,

                PaymentMethod =
                    order.PaymentMethod,

                TotalPrice =
                    order.TotalPrice,

                DeliveryAddressTitle =
                    order.DeliveryAddressTitle,

                DeliveryAddress =
                    order.DeliveryAddress,

                DeliveryLatitude =
                    order.DeliveryLatitude,

                DeliveryLongitude =
                    order.DeliveryLongitude,

                CustomerNote =
                    order.CustomerNote,

                RecommendationSearchId =
                    order.RecommendationSearchId,

                SuitabilityScore =
                    order.SuitabilityScore,

                CreatedAt =
                    order.CreatedAt,

                StatusUpdatedAt =
                    order.StatusUpdatedAt,

                Items =
                    order.OrderItems
                        .OrderBy(item =>
                            item.Id)
                        .Select(MapOrderItem)
                        .ToList()
            };
        }

        private static AdminOrderItemResponse
            MapOrderItem(
                OrderItem item)
        {
            return new AdminOrderItemResponse
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
            };
        }

        private static
            AdminProducerApplicationResponse
            MapProducerApplication(
                ProducerProfile application)
        {
            return new
                AdminProducerApplicationResponse
            {
                ProducerProfileId =
                    application.Id,

                UserId =
                    application.UserId,

                FullName =
                    application.User.FullName,

                Email =
                    application.User.Email,

                UserRole =
                    application.User.Role,

                BusinessName =
                    application.BusinessName,

                Description =
                    application.Description,

                Address =
                    application.Address,

                Latitude =
                    application.Latitude,

                Longitude =
                    application.Longitude,

                DailyCapacity =
                    application.DailyCapacity,

                RemainingCapacity =
                    application.RemainingCapacity,

                IsAvailable =
                    application.IsAvailable,

                IsApproved =
                    application.IsApproved,

                VerificationStatus =
                    application.VerificationStatus,

                CreatedAt =
                    application.CreatedAt,

                ApprovedAt =
                    application.ApprovedAt,

                ApprovedByAdminId =
                    application.ApprovedByAdminId,

                RejectedAt =
                    application.RejectedAt,

                RejectedByAdminId =
                    application.RejectedByAdminId,

                RejectionReason =
                    application.RejectionReason
            };
        }

        private static AdminUserListItemResponse
            MapUserListItem(
                User user)
        {
            return new AdminUserListItemResponse
            {
                UserId =
                    user.Id,

                FullName =
                    user.FullName,

                Email =
                    user.Email,

                Phone =
                    user.Phone,

                Role =
                    user.Role,

                IsActive =
                    user.IsActive,

                CreatedAt =
                    user.CreatedAt,

                ProducerProfileId =
                    user.ProducerProfile?.Id,

                BusinessName =
                    user.ProducerProfile
                        ?.BusinessName,

                ProducerVerificationStatus =
                    user.ProducerProfile
                        ?.VerificationStatus
            };
        }

        private static AdminUserDetailResponse
            MapUserDetail(
                User user)
        {
            return new AdminUserDetailResponse
            {
                UserId =
                    user.Id,

                FullName =
                    user.FullName,

                Email =
                    user.Email,

                Phone =
                    user.Phone,

                Role =
                    user.Role,

                IsActive =
                    user.IsActive,

                CreatedAt =
                    user.CreatedAt,

                AddressCount =
                    user.Addresses.Count,

                OrderCount =
                    user.Orders.Count,

                ReviewCount =
                    user.Reviews.Count,

                FavoriteCount =
                    user.Favorites.Count,

                ProducerProfileId =
                    user.ProducerProfile?.Id,

                BusinessName =
                    user.ProducerProfile
                        ?.BusinessName,

                ProducerVerificationStatus =
                    user.ProducerProfile
                        ?.VerificationStatus,

                IsProducerApproved =
                    user.ProducerProfile
                        ?.IsApproved,

                IsProducerAvailable =
                    user.ProducerProfile
                        ?.IsAvailable,

                DailyCapacity =
                    user.ProducerProfile
                        ?.DailyCapacity,

                RemainingCapacity =
                    user.ProducerProfile
                        ?.RemainingCapacity
            };
        }

        private static string?
            NormalizeVerificationStatus(
                string verificationStatus)
        {
            var normalizedValue =
                verificationStatus.Trim();

            if (normalizedValue.Equals(
                    ProducerVerificationStatuses
                        .Pending,

                    StringComparison
                        .OrdinalIgnoreCase))
            {
                return
                    ProducerVerificationStatuses
                        .Pending;
            }

            if (normalizedValue.Equals(
                    ProducerVerificationStatuses
                        .Approved,

                    StringComparison
                        .OrdinalIgnoreCase))
            {
                return
                    ProducerVerificationStatuses
                        .Approved;
            }

            if (normalizedValue.Equals(
                    ProducerVerificationStatuses
                        .Rejected,

                    StringComparison
                        .OrdinalIgnoreCase))
            {
                return
                    ProducerVerificationStatuses
                        .Rejected;
            }

            return null;
        }

        private static string?
            NormalizeUserRole(
                string role)
        {
            var normalizedValue =
                role.Trim();

            if (normalizedValue.Equals(
                    UserRoles.Customer,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                return UserRoles.Customer;
            }

            if (normalizedValue.Equals(
                    UserRoles.Producer,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                return UserRoles.Producer;
            }

            if (normalizedValue.Equals(
                    UserRoles.Admin,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                return UserRoles.Admin;
            }

            return null;
        }
        private static string?
    NormalizeOrderStatus(
        string status)
        {
            var normalizedValue =
                status.Trim();

            var allowedStatuses =
                new[]
                {
            OrderStatuses.Pending,
            OrderStatuses.Accepted,
            OrderStatuses.Preparing,
            OrderStatuses.Ready,
            OrderStatuses.OutForDelivery,
            OrderStatuses.Delivered,
            OrderStatuses.Rejected,
            OrderStatuses.Cancelled
                };

            return allowedStatuses
                .FirstOrDefault(
                    allowedStatus =>
                        allowedStatus.Equals(
                            normalizedValue,
                            StringComparison
                                .OrdinalIgnoreCase));
        }
    }
}