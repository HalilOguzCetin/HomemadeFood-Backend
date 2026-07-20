using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var favorites =
                await _favoriteService
                    .GetMyFavoritesAsync(userId);

            return Ok(
                ApiResponse<object>.Succeed(
                    favorites,
                    "Favoriler başarıyla getirildi."));
        }

        [HttpPost("{foodId:int}")]
        public async Task<IActionResult> AddFavorite(
            int foodId)
        {
            if (foodId <= 0)
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
                await _favoriteService
                    .AddFavoriteAsync(
                        userId,
                        foodId);

            if (!result)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .FavoriteAdditionFailed,
                        "Yemek bulunamadı, satışta değil veya zaten favorilerinizde."));
            }

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<object>.Succeed(
                    new
                    {
                        foodId
                    },
                    "Yemek favorilere başarıyla eklendi.",
                    ApiResponseCodes.Created));
        }

        [HttpDelete("{foodId:int}")]
        public async Task<IActionResult> RemoveFavorite(
            int foodId)
        {
            if (foodId <= 0)
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
                await _favoriteService
                    .RemoveFavoriteAsync(
                        userId,
                        foodId);

            if (!result)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .FavoriteRemovalFailed,
                        "Yemek favorilerinizde bulunamadı."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        foodId
                    },
                    "Yemek favorilerden başarıyla çıkarıldı."));
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