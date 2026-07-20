using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.DTOs.Food;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Producer)]
    public class FoodController : ControllerBase
    {
        private readonly IFoodService _foodService;

        public FoodController(
            IFoodService foodService)
        {
            _foodService = foodService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAvailableFoods(
            [FromQuery] int? categoryId,
            [FromQuery] string? search)
        {
            if (categoryId.HasValue &&
                categoryId.Value <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Kategori ID değeri sıfırdan büyük olmalıdır."));
            }

            var foods =
                await _foodService
                    .GetAvailableFoodsAsync(
                        categoryId,
                        search);

            return Ok(
                ApiResponse<object>.Succeed(
                    foods,
                    "Yemekler başarıyla getirildi."));
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetFoodById(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Yemek ID değeri sıfırdan büyük olmalıdır."));
            }

            var food =
                await _foodService
                    .GetAvailableFoodByIdAsync(id);

            if (food == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.FoodNotFound,
                        "Yemek bulunamadı veya şu anda satışta değil."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    food,
                    "Yemek başarıyla getirildi."));
        }

        [HttpPost]
        public async Task<IActionResult> CreateFood(
            [FromBody] CreateFoodRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var food =
                await _foodService.CreateFoodAsync(
                    userId,
                    request);

            if (food == null)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.FoodCreationFailed,
                        "Yemek eklenemedi. Üretici profili veya kategori geçersiz olabilir."));
            }

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<object>.Succeed(
                    food,
                    "Yemek başarıyla oluşturuldu.",
                    ApiResponseCodes.Created));
        }

        [HttpGet("my-foods")]
        public async Task<IActionResult> GetMyFoods()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var foods =
                await _foodService
                    .GetMyFoodsAsync(userId);

            return Ok(
                ApiResponse<object>.Succeed(
                    foods,
                    "Üretici yemekleri başarıyla getirildi."));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFood(
            int id,
            [FromBody] UpdateFoodRequest request)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Yemek ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var food =
                await _foodService.UpdateFoodAsync(
                    userId,
                    id,
                    request);

            if (food == null)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.FoodUpdateFailed,
                        "Yemek güncellenemedi. Yemek bulunamamış, size ait olmayabilir veya kategori geçersiz olabilir."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    food,
                    "Yemek başarıyla güncellendi."));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFood(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Yemek ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var result =
                await _foodService.DeleteFoodAsync(
                    userId,
                    id);

            if (!result)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.FoodDeletionFailed,
                        "Yemek bulunamadı veya bu yemek size ait değil."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        foodId = id
                    },
                    "Yemek başarıyla satıştan kaldırıldı."));
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