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
        [Authorize(
    Policy =
        AuthorizationPolicies.ApprovedProducer)]
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var profile =
                await _producerService
                    .GetMyApplicationAsync(userId);

            if (profile == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.NotFound,
                        "Üretici profili bulunamadı."));
            }

            if (!profile.IsApproved)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Üretici profili henüz onaylanmamış."));
            }

            return Ok(
                ApiResponse<ProducerApplicationStatusResponse>
                    .Succeed(
                        profile,
                        "Üretici profili başarıyla getirildi."));
        }
        [Authorize(
            Policy =
                AuthorizationPolicies.ApprovedProducer)]
        [HttpPut("my-profile")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> UpdateMyProfile(
            [FromForm]
            UpdateProducerProfileRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            ProducerApplicationStatusResponse?
                updatedProfile;

            try
            {
                updatedProfile =
                    await _producerService
                        .UpdateMyProfileAsync(
                            userId,
                            request);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        exception.Message));
            }

            if (updatedProfile == null)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Üretici profili güncellenemedi. Profil bulunamamış, onaylanmamış veya aynı anda başka bir işlemle değiştirilmiş olabilir."));
            }

            return Ok(
                ApiResponse<
                    ProducerApplicationStatusResponse>
                    .Succeed(
                        updatedProfile,
                        "Üretici profili başarıyla güncellendi."));
        }

        [Authorize(Roles = UserRoles.Customer)]
        [HttpPost("apply")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> Apply(
            [FromForm] ProducerApplicationRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            bool result;

            try
            {
                result =
                    await _producerService.ApplyAsync(
                        userId,
                        request);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        exception.Message));
            }

            if (!result)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .ProducerApplicationFailed,
                        "Başvuru oluşturulamadı. Daha önce başvuru yapmış olabilirsiniz veya bilgiler geçersizdir."));
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
        [Authorize(Roles = UserRoles.Customer)]
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