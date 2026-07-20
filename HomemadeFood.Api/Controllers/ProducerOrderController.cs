using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.DTOs.ProducerOrder;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Producer)]
    public class ProducerOrderController : ControllerBase
    {
        private readonly IProducerOrderService
            _producerOrderService;

        public ProducerOrderController(
            IProducerOrderService producerOrderService)
        {
            _producerOrderService =
                producerOrderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            if (!TryGetUserId(out var producerUserId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Üretici bilgisi alınamadı."));
            }

            var orders =
                await _producerOrderService
                    .GetMyOrdersAsync(
                        producerUserId);

            return Ok(
                ApiResponse<List<ProducerOrderResponse>>
                    .Succeed(
                        orders,
                        "Üretici siparişleri başarıyla getirildi."));
        }

        [HttpPut("{id:int}/accept")]
        public async Task<IActionResult> AcceptOrder(
            int id)
        {
            if (!IsValidOrderId(id))
            {
                return InvalidOrderIdResponse();
            }

            if (!TryGetUserId(out var producerUserId))
            {
                return UnauthorizedProducerResponse();
            }

            var order =
                await _producerOrderService
                    .AcceptOrderAsync(
                        producerUserId,
                        id);

            if (order == null)
            {
                return BadRequest(
                    ApiResponse<ProducerOrderResponse>.Fail(
                        ApiResponseCodes
                            .OrderAcceptanceFailed,
                        "Sipariş kabul edilemedi. Sipariş bulunamamış, size ait olmayabilir veya durumu Pending olmayabilir."));
            }

            return Ok(
                ApiResponse<ProducerOrderResponse>.Succeed(
                    order,
                    "Sipariş başarıyla kabul edildi."));
        }

        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> RejectOrder(
            int id)
        {
            if (!IsValidOrderId(id))
            {
                return InvalidOrderIdResponse();
            }

            if (!TryGetUserId(out var producerUserId))
            {
                return UnauthorizedProducerResponse();
            }

            var order =
                await _producerOrderService
                    .RejectOrderAsync(
                        producerUserId,
                        id);

            if (order == null)
            {
                return BadRequest(
                    ApiResponse<ProducerOrderResponse>.Fail(
                        ApiResponseCodes
                            .OrderRejectionFailed,
                        "Sipariş reddedilemedi. Sipariş bulunamamış, size ait olmayabilir veya durumu Pending olmayabilir."));
            }

            return Ok(
                ApiResponse<ProducerOrderResponse>.Succeed(
                    order,
                    "Sipariş başarıyla reddedildi."));
        }

        [HttpPut("{id:int}/start-preparing")]
        public async Task<IActionResult> StartPreparing(
            int id)
        {
            if (!IsValidOrderId(id))
            {
                return InvalidOrderIdResponse();
            }

            if (!TryGetUserId(out var producerUserId))
            {
                return UnauthorizedProducerResponse();
            }

            var order =
                await _producerOrderService
                    .StartPreparingAsync(
                        producerUserId,
                        id);

            if (order == null)
            {
                return BadRequest(
                    ApiResponse<ProducerOrderResponse>.Fail(
                        ApiResponseCodes
                            .OrderPreparationFailed,
                        "Hazırlama işlemi başlatılamadı. Sipariş bulunamamış, size ait olmayabilir veya durumu Accepted olmayabilir."));
            }

            return Ok(
                ApiResponse<ProducerOrderResponse>.Succeed(
                    order,
                    "Sipariş hazırlanmaya başlandı."));
        }

        [HttpPut("{id:int}/ready")]
        public async Task<IActionResult> MarkReady(
            int id)
        {
            if (!IsValidOrderId(id))
            {
                return InvalidOrderIdResponse();
            }

            if (!TryGetUserId(out var producerUserId))
            {
                return UnauthorizedProducerResponse();
            }

            var order =
                await _producerOrderService
                    .MarkReadyAsync(
                        producerUserId,
                        id);

            if (order == null)
            {
                return BadRequest(
                    ApiResponse<ProducerOrderResponse>.Fail(
                        ApiResponseCodes
                            .OrderReadyFailed,
                        "Sipariş hazır durumuna getirilemedi. Sipariş bulunamamış, size ait olmayabilir veya durumu Preparing olmayabilir."));
            }

            return Ok(
                ApiResponse<ProducerOrderResponse>.Succeed(
                    order,
                    "Sipariş hazır durumuna getirildi."));
        }

        [HttpPut("{id:int}/out-for-delivery")]
        public async Task<IActionResult>
            MarkOutForDelivery(
                int id)
        {
            if (!IsValidOrderId(id))
            {
                return InvalidOrderIdResponse();
            }

            if (!TryGetUserId(out var producerUserId))
            {
                return UnauthorizedProducerResponse();
            }

            var order =
                await _producerOrderService
                    .MarkOutForDeliveryAsync(
                        producerUserId,
                        id);

            if (order == null)
            {
                return BadRequest(
                    ApiResponse<ProducerOrderResponse>.Fail(
                        ApiResponseCodes
                            .OrderDeliveryStartFailed,
                        "Sipariş teslimata çıkarılamadı. Sipariş bulunamamış, size ait olmayabilir veya durumu Ready olmayabilir."));
            }

            return Ok(
                ApiResponse<ProducerOrderResponse>.Succeed(
                    order,
                    "Sipariş başarıyla teslimata çıkarıldı."));
        }

        [HttpPut("{id:int}/delivered")]
        public async Task<IActionResult> MarkDelivered(
            int id)
        {
            if (!IsValidOrderId(id))
            {
                return InvalidOrderIdResponse();
            }

            if (!TryGetUserId(out var producerUserId))
            {
                return UnauthorizedProducerResponse();
            }

            var order =
                await _producerOrderService
                    .MarkDeliveredAsync(
                        producerUserId,
                        id);

            if (order == null)
            {
                return BadRequest(
                    ApiResponse<ProducerOrderResponse>.Fail(
                        ApiResponseCodes
                            .OrderDeliveryCompletionFailed,
                        "Sipariş teslim edildi olarak işaretlenemedi. Sipariş bulunamamış, size ait olmayabilir veya durumu OutForDelivery olmayabilir."));
            }

            return Ok(
                ApiResponse<ProducerOrderResponse>.Succeed(
                    order,
                    "Sipariş başarıyla teslim edildi olarak işaretlendi."));
        }

        private IActionResult InvalidOrderIdResponse()
        {
            return BadRequest(
                ApiResponse<object>.Fail(
                    ApiResponseCodes.BadRequest,
                    "Sipariş ID değeri sıfırdan büyük olmalıdır."));
        }

        private IActionResult UnauthorizedProducerResponse()
        {
            return Unauthorized(
                ApiResponse<object>.Fail(
                    ApiResponseCodes.Unauthorized,
                    "Üretici bilgisi alınamadı."));
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

        private static bool IsValidOrderId(
            int orderId)
        {
            return orderId > 0;
        }
    }
}