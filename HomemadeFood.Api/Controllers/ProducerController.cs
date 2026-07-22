using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.DTOs.Producer;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProducerController : ControllerBase
    {
        private readonly IProducerService
            _producerService;

        public ProducerController(
            IProducerService producerService)
        {
            _producerService = producerService;
        }

        [Authorize(Roles = UserRoles.Customer)]
        [HttpPost("apply")]
        public async Task<IActionResult> Apply(
            [FromBody] ProducerApplicationRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var result =
                await _producerService.ApplyAsync(
                    userId,
                    request);

            if (!result)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .ProducerApplicationFailed,
                        "Başvuru oluşturulamadı. Daha önce başvuru yapmış olabilirsiniz veya kapasite bilgisi geçersizdir."));
            }

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<object>.Succeed(
                    new
                    {
                        applicationSubmitted = true,

                        verificationStatus =
                            ProducerVerificationStatuses
                                .Pending
                    },
                    "Üretici başvurusu başarıyla oluşturuldu. Admin onayı bekleniyor.",
                    ApiResponseCodes.Created));
        }
        [Authorize(
    Roles =
        UserRoles.Customer + "," +
        UserRoles.Producer)]
        [HttpGet("my-application")]
        public async Task<IActionResult>
    GetMyApplication()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var application =
                await _producerService
                    .GetMyApplicationAsync(userId);

            if (application == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.NotFound,
                        "Bu kullanıcıya ait bir üretici başvurusu bulunamadı."));
            }

            return Ok(
                ApiResponse<ProducerApplicationStatusResponse>
                    .Succeed(
                        application,
                        "Üretici başvurusu başarıyla getirildi."));
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