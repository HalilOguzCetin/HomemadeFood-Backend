using HomemadeFood.Api.DTOs.Food;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Producer")]
    public class FoodController : ControllerBase
    {
        private readonly IFoodService _foodService;

        public FoodController(IFoodService foodService)
        {
            _foodService = foodService;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAvailableFoods(
    [FromQuery] int? categoryId,
    [FromQuery] string? search)
        {
            if (categoryId.HasValue && categoryId.Value <= 0)
            {
                return BadRequest(
                    "Kategori ID değeri sıfırdan büyük olmalıdır.");
            }

            var foods =
                await _foodService
                    .GetAvailableFoodsAsync(
                        categoryId,
                        search);

            return Ok(foods);
        }
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetFoodById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "Yemek ID değeri sıfırdan büyük olmalıdır.");
            }

            var food =
                await _foodService
                    .GetAvailableFoodByIdAsync(id);

            if (food == null)
            {
                return NotFound(
                    "Yemek bulunamadı veya şu anda satışta değil.");
            }

            return Ok(food);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFood(
            [FromBody] CreateFoodRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized("Kullanıcı bilgisi alınamadı.");
            }

            var food = await _foodService.CreateFoodAsync(
                userId,
                request);

            if (food == null)
            {
                return BadRequest(
                    "Yemek eklenemedi. Üretici profili veya kategori geçersiz.");
            }

            return StatusCode(201, food);
        }

        [HttpGet("my-foods")]
        public async Task<IActionResult> GetMyFoods()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized("Kullanıcı bilgisi alınamadı.");
            }

            var foods = await _foodService.GetMyFoodsAsync(userId);

            return Ok(foods);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFood(
            int id,
            [FromBody] UpdateFoodRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized("Kullanıcı bilgisi alınamadı.");
            }

            var food = await _foodService.UpdateFoodAsync(
                userId,
                id,
                request);

            if (food == null)
            {
                return BadRequest(
                    "Yemek bulunamadı, size ait değil veya kategori geçersiz.");
            }

            return Ok(food);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFood(int id)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized("Kullanıcı bilgisi alınamadı.");
            }

            var result = await _foodService.DeleteFoodAsync(
                userId,
                id);

            if (!result)
            {
                return NotFound(
                    "Yemek bulunamadı veya bu yemek size ait değil.");
            }

            return Ok("Yemek satıştan kaldırıldı.");
        }

        private bool TryGetUserId(out int userId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userIdValue, out userId);
        }
    }
}