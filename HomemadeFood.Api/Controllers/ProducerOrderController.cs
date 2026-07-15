using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Producer")]
    public class ProducerOrderController : ControllerBase
    {
        private readonly IProducerOrderService _producerOrderService;

        public ProducerOrderController(
            IProducerOrderService producerOrderService)
        {
            _producerOrderService = producerOrderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            if (!TryGetUserId(out var producerUserId))
            {
                return Unauthorized(
                    "Üretici bilgisi alınamadı.");
            }

            var orders =
                await _producerOrderService
                    .GetMyOrdersAsync(producerUserId);

            return Ok(orders);
        }

        [HttpPut("{id:int}/accept")]
        public async Task<IActionResult> AcceptOrder(int id)
        {
            if (!IsValidOrderId(id))
            {
                return BadRequest(
                    "Sipariş ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var producerUserId))
            {
                return Unauthorized(
                    "Üretici bilgisi alınamadı.");
            }

            var order =
                await _producerOrderService
                    .AcceptOrderAsync(
                        producerUserId,
                        id);

            if (order == null)
            {
                return BadRequest(
                    "Sipariş kabul edilemedi. Sipariş bulunamamış, size ait olmayabilir veya durumu Pending olmayabilir.");
            }

            return Ok(order);
        }

        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> RejectOrder(int id)
        {
            if (!IsValidOrderId(id))
            {
                return BadRequest(
                    "Sipariş ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var producerUserId))
            {
                return Unauthorized(
                    "Üretici bilgisi alınamadı.");
            }

            var order =
                await _producerOrderService
                    .RejectOrderAsync(
                        producerUserId,
                        id);

            if (order == null)
            {
                return BadRequest(
                    "Sipariş reddedilemedi. Sipariş bulunamamış, size ait olmayabilir veya durumu Pending olmayabilir.");
            }

            return Ok(order);
        }

        [HttpPut("{id:int}/start-preparing")]
        public async Task<IActionResult> StartPreparing(int id)
        {
            if (!IsValidOrderId(id))
            {
                return BadRequest(
                    "Sipariş ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var producerUserId))
            {
                return Unauthorized(
                    "Üretici bilgisi alınamadı.");
            }

            var order =
                await _producerOrderService
                    .StartPreparingAsync(
                        producerUserId,
                        id);

            if (order == null)
            {
                return BadRequest(
                    "Hazırlama işlemi başlatılamadı. Siparişin durumu Accepted olmalıdır.");
            }

            return Ok(order);
        }

        [HttpPut("{id:int}/ready")]
        public async Task<IActionResult> MarkReady(int id)
        {
            if (!IsValidOrderId(id))
            {
                return BadRequest(
                    "Sipariş ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var producerUserId))
            {
                return Unauthorized(
                    "Üretici bilgisi alınamadı.");
            }

            var order =
                await _producerOrderService
                    .MarkReadyAsync(
                        producerUserId,
                        id);

            if (order == null)
            {
                return BadRequest(
                    "Sipariş hazır durumuna getirilemedi. Siparişin durumu Preparing olmalıdır.");
            }

            return Ok(order);
        }

        [HttpPut("{id:int}/out-for-delivery")]
        public async Task<IActionResult> MarkOutForDelivery(int id)
        {
            if (!IsValidOrderId(id))
            {
                return BadRequest(
                    "Sipariş ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var producerUserId))
            {
                return Unauthorized(
                    "Üretici bilgisi alınamadı.");
            }

            var order =
                await _producerOrderService
                    .MarkOutForDeliveryAsync(
                        producerUserId,
                        id);

            if (order == null)
            {
                return BadRequest(
                    "Sipariş teslimata çıkarılamadı. Siparişin durumu Ready olmalıdır.");
            }

            return Ok(order);
        }

        [HttpPut("{id:int}/delivered")]
        public async Task<IActionResult> MarkDelivered(int id)
        {
            if (!IsValidOrderId(id))
            {
                return BadRequest(
                    "Sipariş ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var producerUserId))
            {
                return Unauthorized(
                    "Üretici bilgisi alınamadı.");
            }

            var order =
                await _producerOrderService
                    .MarkDeliveredAsync(
                        producerUserId,
                        id);

            if (order == null)
            {
                return BadRequest(
                    "Sipariş teslim edildi olarak işaretlenemedi. Siparişin durumu OutForDelivery olmalıdır.");
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

        private static bool IsValidOrderId(int orderId)
        {
            return orderId > 0;
        }
    }
}