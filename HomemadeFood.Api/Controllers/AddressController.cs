using HomemadeFood.Api.DTOs.Address;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
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
                    "Kullanıcı bilgisi alınamadı.");
            }

            var address =
                await _addressService.CreateAddressAsync(
                    userId,
                    request);

            return StatusCode(
                StatusCodes.Status201Created,
                address);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var addresses =
                await _addressService.GetMyAddressesAsync(
                    userId);

            return Ok(addresses);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAddress(
            int id,
            [FromBody] UpdateAddressRequest request)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "Adres ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var address =
                await _addressService.UpdateAddressAsync(
                    userId,
                    id,
                    request);

            if (address == null)
            {
                return NotFound(
                    "Adres bulunamadı veya bu adres size ait değil.");
            }

            return Ok(address);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "Adres ID değeri sıfırdan büyük olmalıdır.");
            }

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    "Kullanıcı bilgisi alınamadı.");
            }

            var result =
                await _addressService.DeleteAddressAsync(
                    userId,
                    id);

            if (!result)
            {
                return NotFound(
                    "Adres bulunamadı veya bu adres size ait değil.");
            }

            return Ok("Adres başarıyla silindi.");
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