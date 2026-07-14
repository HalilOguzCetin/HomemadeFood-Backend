using HomemadeFood.Api.DTOs.Cart;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var cart =
                await _cartService.GetCartAsync(userId);

            return Ok(cart);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem(
            [FromBody] AddCartItemRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var cart =
                await _cartService.AddItemAsync(
                    userId,
                    request);

            if (cart == null)
            {
                return BadRequest(
                    "Yemek sepete eklenemedi. Yemek satışta olmayabilir, miktar sınırı aşılmış olabilir veya sepette farklı bir üreticinin yemeği bulunabilir.");
            }

            return StatusCode(
                StatusCodes.Status201Created,
                cart);
        }

        [HttpPut("items/{cartItemId:int}")]
        public async Task<IActionResult> UpdateItem(
            int cartItemId,
            [FromBody] UpdateCartItemRequest request)
        {
            if (cartItemId <= 0)
            {
                return BadRequest(
                    "Sepet ürünü ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var cart =
                await _cartService.UpdateItemAsync(
                    userId,
                    cartItemId,
                    request);

            if (cart == null)
            {
                return NotFound(
                    "Sepet veya sepet ürünü bulunamadı.");
            }

            return Ok(cart);
        }

        [HttpDelete("items/{cartItemId:int}")]
        public async Task<IActionResult> RemoveItem(
            int cartItemId)
        {
            if (cartItemId <= 0)
            {
                return BadRequest(
                    "Sepet ürünü ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var cart =
                await _cartService.RemoveItemAsync(
                    userId,
                    cartItemId);

            if (cart == null)
            {
                return NotFound(
                    "Sepet veya sepet ürünü bulunamadı.");
            }

            return Ok(cart);
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var result =
                await _cartService.ClearCartAsync(userId);

            if (!result)
            {
                return NotFound(
                    "Silinecek bir sepet bulunamadı.");
            }

            return Ok("Sepet başarıyla temizlendi.");
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