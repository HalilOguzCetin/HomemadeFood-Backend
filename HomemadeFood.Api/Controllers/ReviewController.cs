using HomemadeFood.Api.DTOs.Review;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
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
                    "Kullanıcı bilgisi alınamadı.");
            }

            var review =
                await _reviewService.CreateReviewAsync(
                    customerId,
                    request);

            if (review == null)
            {
                return BadRequest(
                    "Değerlendirme oluşturulamadı. Sipariş bulunamamış, size ait olmayabilir, teslim edilmemiş olabilir veya daha önce değerlendirilmiş olabilir.");
            }

            return StatusCode(
                StatusCodes.Status201Created,
                review);
        }

        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews()
        {
            if (!TryGetUserId(out var customerId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var reviews =
                await _reviewService
                    .GetMyReviewsAsync(customerId);

            return Ok(reviews);
        }

        [AllowAnonymous]
        [HttpGet("producer/{producerProfileId:int}")]
        public async Task<IActionResult> GetProducerReviews(
            int producerProfileId)
        {
            if (producerProfileId <= 0)
            {
                return BadRequest(
                    "Üretici profil ID değeri sıfırdan büyük olmalıdır.");
            }

            var reviews =
                await _reviewService
                    .GetProducerReviewsAsync(
                        producerProfileId);

            return Ok(reviews);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "Değerlendirme ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var customerId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var result =
                await _reviewService.DeleteReviewAsync(
                    customerId,
                    id);

            if (!result)
            {
                return NotFound(
                    "Değerlendirme bulunamadı veya bu değerlendirme size ait değil.");
            }

            return Ok(
                "Değerlendirme başarıyla silindi.");
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