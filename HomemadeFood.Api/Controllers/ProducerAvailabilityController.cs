using System.Security.Claims;
using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.DTOs.Producer;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/Producer")]
    [Authorize(
        Policy =
            AuthorizationPolicies
                .ApprovedProducer)]
    public sealed class ProducerAvailabilityController
        : ControllerBase
    {
        private readonly
            IProducerAvailabilityService
            _producerAvailabilityService;

        public ProducerAvailabilityController(
            IProducerAvailabilityService
                producerAvailabilityService)
        {
            _producerAvailabilityService =
                producerAvailabilityService;
        }

        [HttpGet("my-availability")]
        public async Task<IActionResult>
            GetMyAvailability()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var response =
                await _producerAvailabilityService
                    .GetMyAvailabilityAsync(
                        userId);

            if (response == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.NotFound,
                        "Onaylı üretici profili bulunamadı."));
            }

            return Ok(
                ApiResponse<
                    ProducerAvailabilityResponse>
                    .Succeed(
                        response,
                        "İşletme çalışma durumu başarıyla getirildi."));
        }

        [HttpPut("my-business-hours")]
        public async Task<IActionResult>
            UpdateMyBusinessHours(
                [FromBody]
                UpdateProducerBusinessHoursRequest
                    request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            ProducerAvailabilityResponse?
                response;

            try
            {
                response =
                    await _producerAvailabilityService
                        .UpdateMyBusinessHoursAsync(
                            userId,
                            request);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .BadRequest,
                        exception.Message));
            }

            if (response == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.NotFound,
                        "Onaylı üretici profili bulunamadı."));
            }

            return Ok(
                ApiResponse<
                    ProducerAvailabilityResponse>
                    .Succeed(
                        response,
                        "İşletme çalışma saatleri başarıyla güncellendi."));
        }

        [HttpPut("my-availability-mode")]
        public async Task<IActionResult>
            UpdateMyAvailabilityMode(
                [FromBody]
                UpdateProducerAvailabilityModeRequest
                    request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            ProducerAvailabilityResponse?
                response;

            try
            {
                response =
                    await _producerAvailabilityService
                        .UpdateMyAvailabilityModeAsync(
                            userId,
                            request);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .BadRequest,
                        exception.Message));
            }

            if (response == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.NotFound,
                        "Onaylı üretici profili bulunamadı."));
            }

            return Ok(
                ApiResponse<
                    ProducerAvailabilityResponse>
                    .Succeed(
                        response,
                        "İşletme çalışma modu başarıyla güncellendi."));
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