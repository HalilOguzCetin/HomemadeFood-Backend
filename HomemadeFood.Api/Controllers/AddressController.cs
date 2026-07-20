using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Address;
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
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(
            IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress(
            [FromBody] CreateAddressRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var address =
                await _addressService.CreateAddressAsync(
                    userId,
                    request);

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<object>.Succeed(
                    address,
                    "Adres başarıyla oluşturuldu.",
                    ApiResponseCodes.Created));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var addresses =
                await _addressService.GetMyAddressesAsync(
                    userId);

            return Ok(
                ApiResponse<object>.Succeed(
                    addresses,
                    "Adresler başarıyla getirildi."));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAddress(
            int id,
            [FromBody] UpdateAddressRequest request)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Adres ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var address =
                await _addressService.UpdateAddressAsync(
                    userId,
                    id,
                    request);

            if (address == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.NotFound,
                        "Adres bulunamadı veya bu adres size ait değil."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    address,
                    "Adres başarıyla güncellendi."));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAddress(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Adres ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var result =
                await _addressService.DeleteAddressAsync(
                    userId,
                    id);

            if (!result)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.NotFound,
                        "Adres bulunamadı veya bu adres size ait değil."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        addressId = id
                    },
                    "Adres başarıyla silindi."));
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