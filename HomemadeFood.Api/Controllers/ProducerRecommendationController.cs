using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.DTOs.Recommendation;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Customer)]
    public sealed class ProducerRecommendationController
        : ControllerBase
    {
        private readonly IProducerRecommendationService
            _recommendationService;

        public ProducerRecommendationController(
            IProducerRecommendationService recommendationService)
        {
            _recommendationService =
                recommendationService;
        }

        [HttpPost("recommend")]
        public async Task<IActionResult> GetRecommendations(
    [FromBody] ProducerRecommendationRequest request)
        {
            if (!TryGetUserId(out var customerUserId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Müşteri bilgisi alınamadı."));
            }

            var result =
                await _recommendationService
                    .GetRecommendationsAsync(
                        customerUserId,
                        request);

            if (result == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.NotFound,
                        "Adres bulunamadı veya bu adres giriş yapan kullanıcıya ait değil."));
            }

            var message =
                result.Recommendations.Count == 0
                    ? "Arama kriterlerine uygun üretici bulunamadı."
                    : $"{result.Recommendations.Count} üretici önerisi başarıyla oluşturuldu.";

            return Ok(
                ApiResponse<ProducerRecommendationResultResponse>
                    .Succeed(
                        result,
                        message));
        }
        [HttpPost("select")]
        public async Task<IActionResult> SelectRecommendation(
    [FromBody]
    ProducerRecommendationSelectionRequest request)
        {
            if (!TryGetUserId(out var customerUserId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Müşteri bilgisi alınamadı."));
            }

            var result =
                await _recommendationService
                    .SelectRecommendationAsync(
                        customerUserId,
                        request);

            if (!result.IsSuccess)
            {
                if (result.Code ==
                    ApiResponseCodes
                        .RecommendationSearchNotFound)
                {
                    return NotFound(
                        ApiResponse<object>.Fail(
                            result.Code,
                            result.Message));
                }

                if (result.Code ==
                    ApiResponseCodes
                        .RecommendationAlreadySelected)
                {
                    return Conflict(
                        ApiResponse<object>.Fail(
                            result.Code,
                            result.Message));
                }

                return BadRequest(
                    ApiResponse<object>.Fail(
                        result.Code,
                        result.Message));
            }

            return Ok(
                ApiResponse<ProducerRecommendationSelectionResponse>
                    .Succeed(
                        result.Data!,
                        result.Message));
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