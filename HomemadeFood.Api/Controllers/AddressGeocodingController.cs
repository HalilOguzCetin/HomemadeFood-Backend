using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Address;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/Address")]
    [Authorize(Roles = UserRoles.Customer)]
    public class AddressGeocodingController : ControllerBase
    {
        private readonly IGoogleGeocodingService _googleGeocodingService;

        public AddressGeocodingController(
            IGoogleGeocodingService googleGeocodingService)
        {
            _googleGeocodingService =
                googleGeocodingService;
        }

        [HttpGet("reverse-geocode")]
        public async Task<IActionResult> ReverseGeocode(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            CancellationToken cancellationToken)
        {
            if (
                latitude is < -90 or > 90 ||
                longitude is < -180 or > 180)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Enlem -90 ile 90, boylam -180 ile 180 arasında olmalıdır."));
            }

            try
            {
                var result =
                    await _googleGeocodingService
                        .ReverseGeocodeAsync(
                            latitude,
                            longitude,
                            cancellationToken);

                if (result == null)
                {
                    return NotFound(
                        ApiResponse<object>.Fail(
                            ApiResponseCodes.GeocodingNotFound,
                            "Seçilen konum için adres bilgisi bulunamadı."));
                }

                return Ok(
                    ApiResponse<ReverseGeocodeResponse>.Succeed(
                        result,
                        "Adres bilgileri başarıyla çözümlendi."));
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                return StatusCode(
                    StatusCodes.Status504GatewayTimeout,
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.GeocodingServiceUnavailable,
                        "Adres servisi zaman aşımına uğradı. Lütfen tekrar deneyin."));
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.GeocodingServiceUnavailable,
                        "Adres servisine şu anda ulaşılamıyor. Lütfen tekrar deneyin."));
            }
        }
    }
}