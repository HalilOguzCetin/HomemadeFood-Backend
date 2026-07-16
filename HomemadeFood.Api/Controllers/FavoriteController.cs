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
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoriteController(
            IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyFavorites()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var favorites =
                await _favoriteService
                    .GetMyFavoritesAsync(userId);

            return Ok(favorites);
        }

        [HttpPost("{foodId:int}")]
        public async Task<IActionResult> AddFavorite(
            int foodId)
        {
            if (foodId <= 0)
            {
                return BadRequest(
                    "Yemek ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var result =
                await _favoriteService
                    .AddFavoriteAsync(
                        userId,
                        foodId);

            if (!result)
            {
                return BadRequest(
                    "Yemek bulunamadı, satışta değil veya zaten favorilerinizde.");
            }

            return StatusCode(
                StatusCodes.Status201Created,
                "Yemek favorilere eklendi.");
        }

        [HttpDelete("{foodId:int}")]
        public async Task<IActionResult> RemoveFavorite(
            int foodId)
        {
            if (foodId <= 0)
            {
                return BadRequest(
                    "Yemek ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var result =
                await _favoriteService
                    .RemoveFavoriteAsync(
                        userId,
                        foodId);

            if (!result)
            {
                return NotFound(
                    "Yemek favorilerinizde bulunamadı.");
            }

            return Ok(
                "Yemek favorilerden çıkarıldı.");
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