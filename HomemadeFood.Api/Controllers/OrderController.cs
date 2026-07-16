using HomemadeFood.Api.DTOs.Order;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HomemadeFood.Api.Constants;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Customer)]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
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
                    "Kullanıcı bilgisi alınamadı.");
            }

            var order =
                await _orderService.CreateOrderAsync(
                    customerId,
                    request);

            if (order == null)
            {
                return BadRequest(
                    "Sipariş oluşturulamadı. Sepet boş olabilir, adres geçersiz olabilir, yemeklerden biri satışta olmayabilir veya üreticinin kapasitesi yetersiz olabilir.");
            }

            return StatusCode(
                StatusCodes.Status201Created,
                order);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            if (!TryGetUserId(out var customerId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var orders =
                await _orderService.GetMyOrdersAsync(
                    customerId);

            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMyOrderById(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "Sipariş ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var customerId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var order =
                await _orderService.GetMyOrderByIdAsync(
                    customerId,
                    id);

            if (order == null)
            {
                return NotFound(
                    "Sipariş bulunamadı veya bu sipariş size ait değil.");
            }

            return Ok(order);
        }
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "Sipariş ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var customerId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var order =
                await _orderService.CancelOrderAsync(
                    customerId,
                    id);

            if (order == null)
            {
                return BadRequest(
                    "Sipariş iptal edilemedi. Sipariş bulunamamış, size ait olmayabilir veya durumu Pending olmayabilir.");
            }

            return Ok(order);
        }

        private bool TryGetUserId(out int userId)
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