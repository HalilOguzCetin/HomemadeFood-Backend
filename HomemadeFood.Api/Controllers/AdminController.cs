using System.Security.Claims;
using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Admin;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService
            _adminService;

        public AdminController(
            IAdminService adminService)
        {
            _adminService =
                adminService;
        }

        [HttpGet("producer-applications")]
        public async Task<IActionResult>
    GetProducerApplications(
        [FromQuery]
        string? status = null)
        {
            if (!TryNormalizeApplicationStatus(
                    status,
                    out var normalizedStatus))
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Başvuru durumu Pending, Approved veya Rejected olmalıdır."));
            }

            var applications =
                await _adminService
                    .GetProducerApplicationsAsync(
                        normalizedStatus);

            return Ok(
                ApiResponse<
                    List<AdminProducerApplicationResponse>>
                    .Succeed(
                        applications,
                        "Üretici başvuruları başarıyla getirildi."));
        }

        [HttpPost(
            "producer-applications/{id:int}/approve")]
        public async Task<IActionResult>
            ApproveProducer(
                int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Üretici başvuru ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetAdminId(
                    out var adminId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Admin bilgisi alınamadı."));
            }

            var result =
                await _adminService
                    .ApproveProducerAsync(
                        id,
                        adminId);

            if (!result)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .ProducerApprovalFailed,
                        "Başvuru bulunamadı veya daha önce işleme alınmış olabilir."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        producerApplicationId =
                            id,

                        approved =
                            true
                    },
                    "Üretici başvurusu başarıyla onaylandı."));
        }
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
    [FromQuery]
    string? role = null,

    [FromQuery]
    bool? isActive = null,

    [FromQuery]
    string? search = null)
        {
            if (!TryNormalizeUserRole(
                    role,
                    out var normalizedRole))
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,

                        "Kullanıcı rolü Customer veya Admin olmalıdır."));
            }

            if (!string.IsNullOrWhiteSpace(search) &&
                search.Trim().Length > 100)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,

                        "Arama metni en fazla 100 karakter olabilir."));
            }

            var users =
                await _adminService
                    .GetUsersAsync(
                        normalizedRole,
                        isActive,
                        search);

            return Ok(
                ApiResponse<
                    List<AdminUserListItemResponse>>
                    .Succeed(
                        users,

                        "Kullanıcılar başarıyla getirildi."));
        }

        [HttpGet("users/{id:int}")]
        public async Task<IActionResult> GetUserById(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,

                        "Kullanıcı ID değeri sıfırdan büyük olmalıdır."));
            }

            var user =
                await _adminService
                    .GetUserByIdAsync(id);

            if (user == null)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,

                        "Kullanıcı bulunamadı."));
            }

            return Ok(
                ApiResponse<AdminUserDetailResponse>
                    .Succeed(
                        user,

                        "Kullanıcı detayı başarıyla getirildi."));
        }
        [HttpGet("orders/{id:int}")]
        public async Task<IActionResult>
    GetOrderById(
        int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,

                        "Sipariş ID değeri sıfırdan büyük olmalıdır."));
            }

            var order =
                await _adminService
                    .GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.OrderNotFound,

                        "Sipariş bulunamadı."));
            }

            return Ok(
                ApiResponse<AdminOrderDetailResponse>
                    .Succeed(
                        order,

                        "Sipariş detayı başarıyla getirildi."));
        }

        [HttpPatch("users/{id:int}/status")]
        public async Task<IActionResult> UpdateUserStatus(
            int id,

            [FromBody]
    UpdateUserStatusRequest request)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,

                        "Kullanıcı ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetAdminId(
                    out var adminId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,

                        "Admin bilgisi alınamadı."));
            }

            if (id == adminId &&
                !request.IsActive)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,

                        "Admin kendi hesabını pasifleştiremez."));
            }

            var updated =
                await _adminService
                    .UpdateUserStatusAsync(
                        id,
                        request.IsActive);

            if (!updated)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,

                        "Kullanıcı bulunamadı veya hesap durumu güncellenemedi."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        userId = id,

                        isActive =
                            request.IsActive
                    },

                    request.IsActive
                        ? "Kullanıcı hesabı aktifleştirildi."
                        : "Kullanıcı hesabı pasifleştirildi."));
        }
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders(
    [FromQuery]
    string? status = null,

    [FromQuery]
    int? customerId = null,

    [FromQuery]
    int? producerProfileId = null,

    [FromQuery]
    string? search = null,

    [FromQuery]
    DateOnly? dateFrom = null,

    [FromQuery]
    DateOnly? dateTo = null)
        {
            if (!TryNormalizeOrderStatus(
                    status,
                    out var normalizedStatus))
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,

                        "Sipariş durumu Pending, Accepted, Preparing, Ready, OutForDelivery, Delivered, Rejected veya Cancelled olmalıdır."));
            }

            if (customerId.HasValue &&
                customerId.Value <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,

                        "Müşteri ID değeri sıfırdan büyük olmalıdır."));
            }

            if (producerProfileId.HasValue &&
                producerProfileId.Value <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,

                        "Üretici profil ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!string.IsNullOrWhiteSpace(search) &&
                search.Trim().Length > 100)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,

                        "Arama metni en fazla 100 karakter olabilir."));
            }

            if (dateFrom.HasValue &&
                dateTo.HasValue &&
                dateFrom.Value > dateTo.Value)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,

                        "Başlangıç tarihi bitiş tarihinden sonra olamaz."));
            }

            var orders =
                await _adminService
                    .GetOrdersAsync(
                        normalizedStatus,
                        customerId,
                        producerProfileId,
                        search,
                        dateFrom,
                        dateTo);

            return Ok(
                ApiResponse<
                    List<AdminOrderListItemResponse>>
                    .Succeed(
                        orders,

                        "Siparişler başarıyla getirildi."));
        }

        [HttpPost(
            "producer-applications/{id:int}/reject")]
        public async Task<IActionResult>
            RejectProducer(
                int id,
                [FromBody]
                RejectProducerApplicationRequest request)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Üretici başvuru ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetAdminId(
                    out var adminId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Admin bilgisi alınamadı."));
            }

            var rejectionReason =
                request.Reason.Trim();

            var result =
                await _adminService
                    .RejectProducerAsync(
                        id,
                        adminId,
                        rejectionReason);

            if (!result)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .ProducerRejectionFailed,
                        "Başvuru bulunamadı veya daha önce işleme alınmış olabilir."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        producerApplicationId =
                            id,

                        rejected =
                            true,

                        reason =
                            rejectionReason
                    },
                    "Üretici başvurusu başarıyla reddedildi."));
        }
        private static bool TryNormalizeUserRole(
    string? role,
    out string? normalizedRole)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                normalizedRole = null;
                return true;
            }

            var normalizedValue =
                role.Trim();

            if (normalizedValue.Equals(
                    UserRoles.Customer,
                    StringComparison.OrdinalIgnoreCase))
            {
                normalizedRole =
                    UserRoles.Customer;

                return true;
            }

            if (normalizedValue.Equals(
                    UserRoles.Admin,
                    StringComparison.OrdinalIgnoreCase))
            {
                normalizedRole =
                    UserRoles.Admin;

                return true;
            }

            normalizedRole = null;
            return false;
        }
        private static bool TryNormalizeOrderStatus(
    string? status,
    out string? normalizedStatus)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                normalizedStatus =
                    null;

                return true;
            }

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

            normalizedStatus =
                allowedStatuses
                    .FirstOrDefault(
                        allowedStatus =>
                            allowedStatus.Equals(
                                normalizedValue,

                                StringComparison
                                    .OrdinalIgnoreCase));

            return normalizedStatus != null;
        }
        private static bool
    TryNormalizeApplicationStatus(
        string? status,
        out string normalizedStatus)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                normalizedStatus =
                    ProducerVerificationStatuses.Pending;

                return true;
            }

            var normalizedValue =
                status.Trim();

            if (normalizedValue.Equals(
                    ProducerVerificationStatuses.Pending,
                    StringComparison.OrdinalIgnoreCase))
            {
                normalizedStatus =
                    ProducerVerificationStatuses.Pending;

                return true;
            }

            if (normalizedValue.Equals(
                    ProducerVerificationStatuses.Approved,
                    StringComparison.OrdinalIgnoreCase))
            {
                normalizedStatus =
                    ProducerVerificationStatuses.Approved;

                return true;
            }

            if (normalizedValue.Equals(
                    ProducerVerificationStatuses.Rejected,
                    StringComparison.OrdinalIgnoreCase))
            {
                normalizedStatus =
                    ProducerVerificationStatuses.Rejected;

                return true;
            }

            normalizedStatus =
                string.Empty;

            return false;
        }

        private bool TryGetAdminId(
            out int adminId)
        {
            var adminIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            return int.TryParse(
                adminIdValue,
                out adminId);
        }
    }
}