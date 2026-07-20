using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.DTOs.Review;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Customer)]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(
            IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview(
            [FromBody] CreateReviewRequest request)
        {
            if (!TryGetUserId(out var customerId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var review =
                await _reviewService.CreateReviewAsync(
                    customerId,
                    request);

            if (review == null)
            {
                return BadRequest(
                    ApiResponse<ReviewResponse>.Fail(
                        ApiResponseCodes
                            .ReviewCreationFailed,
                        "Değerlendirme oluşturulamadı. Sipariş bulunamamış, size ait olmayabilir, teslim edilmemiş veya daha önce değerlendirilmiş olabilir."));
            }

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<ReviewResponse>.Succeed(
                    review,
                    "Değerlendirme başarıyla oluşturuldu.",
                    ApiResponseCodes.Created));
        }

        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews()
        {
            if (!TryGetUserId(out var customerId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var reviews =
                await _reviewService
                    .GetMyReviewsAsync(customerId);

            return Ok(
                ApiResponse<List<ReviewResponse>>.Succeed(
                    reviews,
                    "Değerlendirmeler başarıyla getirildi."));
        }

        [AllowAnonymous]
        [HttpGet("producer/{producerProfileId:int}")]
        public async Task<IActionResult>
            GetProducerReviews(
                int producerProfileId)
        {
            if (producerProfileId <= 0)
            {
                return BadRequest(
                    ApiResponse<List<ReviewResponse>>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Üretici profil ID değeri sıfırdan büyük olmalıdır."));
            }

            var reviews =
                await _reviewService
                    .GetProducerReviewsAsync(
                        producerProfileId);

            return Ok(
                ApiResponse<List<ReviewResponse>>.Succeed(
                    reviews,
                    "Üretici değerlendirmeleri başarıyla getirildi."));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteReview(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(
    ApiResponse<object>.Fail(
        ApiResponseCodes.BadRequest,
        "Değerlendirme ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetUserId(out var customerId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var result =
                await _reviewService.DeleteReviewAsync(
                    customerId,
                    id);

            if (!result)
            {
                return NotFound(
     ApiResponse<object>.Fail(
         ApiResponseCodes.ReviewDeletionFailed,
         "Değerlendirme bulunamadı veya bu değerlendirme size ait değil."));
            }

            return Ok(
    ApiResponse<object>.Succeed(
        new
        {
            reviewId = id
        },
        "Değerlendirme başarıyla silindi."));
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