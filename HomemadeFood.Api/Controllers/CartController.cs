using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Cart;
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
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(
            ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var cart =
                await _cartService.GetCartAsync(userId);

            return Ok(
                ApiResponse<object>.Succeed(
                    cart,
                    "Sepet başarıyla getirildi."));
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem(
            [FromBody] AddCartItemRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var cart =
                await _cartService.AddItemAsync(
                    userId,
                    request);

            if (cart == null)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.CartItemAdditionFailed,
                        "Yemek sepete eklenemedi. Yemek satışta olmayabilir, miktar sınırı aşılmış olabilir veya sepette farklı bir üreticinin yemeği bulunabilir."));
            }

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<object>.Succeed(
                    cart,
                    "Yemek sepete başarıyla eklendi.",
                    ApiResponseCodes.Created));
        }

        [HttpPut("items/{cartItemId:int}")]
        public async Task<IActionResult> UpdateItem(
            int cartItemId,
            [FromBody] UpdateCartItemRequest request)
        {
            if (cartItemId <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Sepet ürünü ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var cart =
                await _cartService.UpdateItemAsync(
                    userId,
                    cartItemId,
                    request);

            if (cart == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.CartItemUpdateFailed,
                        "Sepet veya sepet ürünü bulunamadı."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    cart,
                    "Sepet ürünü başarıyla güncellendi."));
        }

        [HttpDelete("items/{cartItemId:int}")]
        public async Task<IActionResult> RemoveItem(
            int cartItemId)
        {
            if (cartItemId <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Sepet ürünü ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var cart =
                await _cartService.RemoveItemAsync(
                    userId,
                    cartItemId);

            if (cart == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.CartItemRemovalFailed,
                        "Sepet veya sepet ürünü bulunamadı."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    cart,
                    "Yemek sepetten başarıyla çıkarıldı."));
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var result =
                await _cartService.ClearCartAsync(userId);

            if (!result)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.CartClearFailed,
                        "Temizlenecek bir sepet bulunamadı."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        cleared = true
                    },
                    "Sepet başarıyla temizlendi."));
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