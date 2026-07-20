using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.DTOs.Order;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Customer)]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(
            IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(
            [FromBody] CreateOrderRequest request)
        {
            if (!TryGetUserId(out var customerId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var order =
                await _orderService.CreateOrderAsync(
                    customerId,
                    request);

            if (order == null)
            {
                return BadRequest(
                    ApiResponse<OrderResponse>.Fail(
                        ApiResponseCodes.OrderCreationFailed,
                        "Sipariş oluşturulamadı. Sepet boş, adres geçersiz, yemek satış dışı veya üretici kapasitesi yetersiz olabilir."));
            }

            var response =
                ApiResponse<OrderResponse>.Succeed(
                    order,
                    "Sipariş başarıyla oluşturuldu.",
                    ApiResponseCodes.Created);

            return CreatedAtAction(
                nameof(GetMyOrderById),
                new { id = order.OrderId },
                response);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            if (!TryGetUserId(out var customerId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var orders =
                await _orderService.GetMyOrdersAsync(
                    customerId);

            return Ok(
                ApiResponse<List<OrderResponse>>.Succeed(
                    orders,
                    "Siparişler başarıyla getirildi."));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMyOrderById(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<OrderResponse>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Sipariş ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetUserId(out var customerId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var order =
                await _orderService
                    .GetMyOrderByIdAsync(
                        customerId,
                        id);

            if (order == null)
            {
                return NotFound(
                    ApiResponse<OrderResponse>.Fail(
                        ApiResponseCodes.OrderNotFound,
                        "Sipariş bulunamadı veya bu sipariş size ait değil."));
            }

            return Ok(
                ApiResponse<OrderResponse>.Succeed(
                    order,
                    "Sipariş başarıyla getirildi."));
        }

        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<OrderResponse>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Sipariş ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetUserId(out var customerId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var order =
                await _orderService.CancelOrderAsync(
                    customerId,
                    id);

            if (order == null)
            {
                return BadRequest(
                    ApiResponse<OrderResponse>.Fail(
                        ApiResponseCodes
                            .OrderCancellationFailed,
                        "Sipariş iptal edilemedi. Sipariş bulunamamış, size ait olmayabilir veya durumu Pending olmayabilir."));
            }

            return Ok(
                ApiResponse<OrderResponse>.Succeed(
                    order,
                    "Sipariş başarıyla iptal edildi."));
        }

        private bool TryGetUserId(
            out int userId)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            return int.TryParse(
                userIdValue,
                out userId);
        }
    }
}