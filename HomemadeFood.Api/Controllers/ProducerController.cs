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

        [AllowAnonymous]
        [HttpGet("storefronts")]
        public async Task<IActionResult>
            GetAvailableStorefronts(
                [FromQuery] int? categoryId)
        {
            if (
                categoryId.HasValue &&
                categoryId.Value <= 0
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Kategori ID değeri sıfırdan büyük olmalıdır."));
            }

            var storefronts =
                await _producerService
                    .GetAvailableStorefrontsAsync(
                        categoryId);

            return Ok(
                ApiResponse<
                    List<
                        ProducerStorefrontSummaryResponse>>
                    .Succeed(
                        storefronts,
                        "İşletmeler başarıyla getirildi."));
        }

        /*
         * H4B:
         * Ana sayfadaki Popüler İşletmeler carousel'i.
         *
         * Normal storefront listesiyle karıştırılmaz;
         * gerçek kullanıcı davranışı ayrı endpoint üzerinden
         * hesaplanır.
         */
        [AllowAnonymous]
        [HttpGet("storefronts/popular")]
        public async Task<IActionResult>
            GetPopularStorefronts(
                [FromQuery] int limit = 6)
        {
            if (limit < 1 ||
                limit > 20)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Limit değeri 1 ile 20 arasında olmalıdır."));
            }

            var storefronts =
                await _producerService
                    .GetPopularStorefrontsAsync(
                        limit);

            return Ok(
                ApiResponse<
                    List<
                        PopularProducerStorefrontResponse>>
                    .Succeed(
                        storefronts,
                        "Popüler işletmeler başarıyla getirildi."));
        }
        [AllowAnonymous]
        [HttpGet("storefronts/nearby")]
        public async Task<IActionResult>
            GetNearbyStorefronts(
                [FromQuery] double latitude,
                [FromQuery] double longitude,
                [FromQuery] double radiusKm = 15,
                [FromQuery] int limit = 6)
        {
            if (
                !double.IsFinite(latitude) ||
                latitude < -90 ||
                latitude > 90
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Enlem değeri -90 ile 90 arasında olmalıdır."));
            }

            if (
                !double.IsFinite(longitude) ||
                longitude < -180 ||
                longitude > 180
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Boylam değeri -180 ile 180 arasında olmalıdır."));
            }

            if (
                !double.IsFinite(radiusKm) ||
                radiusKm < 1 ||
                radiusKm > 50
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Yarıçap değeri 1 ile 50 km arasında olmalıdır."));
            }

            if (
                limit < 1 ||
                limit > 20
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Limit değeri 1 ile 20 arasında olmalıdır."));
            }

            var storefronts =
                await _producerService
                    .GetNearbyStorefrontsAsync(
                        latitude,
                        longitude,
                        radiusKm,
                        limit);

            return Ok(
                ApiResponse<
                    List<NearbyProducerStorefrontResponse>>
                    .Succeed(
                        storefronts,
                        "Yakındaki işletmeler başarıyla getirildi."));
        }
        [AllowAnonymous]
        [HttpGet("storefronts/discover")]
        public async Task<IActionResult>
           GetDiscoverStorefronts(
               [FromQuery]
                double latitude,

               [FromQuery]
                double longitude,

               [FromQuery]
                string city,

               [FromQuery]
                double radiusKm = 30,

               [FromQuery]
                int page = 1,

               [FromQuery]
                int pageSize = 20,

               [FromQuery]
                int? categoryId = null,

               [FromQuery]
                string? search = null)
        {
            if (
                !double.IsFinite(
                    latitude) ||
                latitude < -90 ||
                latitude > 90
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .BadRequest,
                        "Enlem değeri -90 ile 90 arasında olmalıdır."));
            }

            if (
                !double.IsFinite(
                    longitude) ||
                longitude < -180 ||
                longitude > 180
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .BadRequest,
                        "Boylam değeri -180 ile 180 arasında olmalıdır."));
            }

            if (
                string.IsNullOrWhiteSpace(
                    city) ||
                city.Trim().Length >
                    100
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .BadRequest,
                        "Şehir bilgisi geçersizdir."));
            }

            if (
                !double.IsFinite(
                    radiusKm) ||
                radiusKm < 1 ||
                radiusKm > 50
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .BadRequest,
                        "Yarıçap değeri 1 ile 50 km arasında olmalıdır."));
            }

            if (page < 1)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .BadRequest,
                        "Sayfa değeri en az 1 olmalıdır."));
            }

            if (
                pageSize < 1 ||
                pageSize > 50
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .BadRequest,
                        "Sayfa boyutu 1 ile 50 arasında olmalıdır."));
            }

            if (
                categoryId.HasValue &&
                categoryId.Value <= 0
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .BadRequest,
                        "Kategori ID değeri sıfırdan büyük olmalıdır."));
            }

            if (
                search != null &&
                search.Length > 100
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .BadRequest,
                        "Arama metni en fazla 100 karakter olabilir."));
            }

            var result =
                await _producerService
                    .GetDiscoverStorefrontsAsync(
                        latitude,
                        longitude,
                        city.Trim(),
                        radiusKm,
                        page,
                        pageSize,
                        categoryId,
                        search);

            return Ok(
                ApiResponse<
                    PagedResultResponse<
                        DiscoverProducerStorefrontResponse>>
                    .Succeed(
                        result,
                        "Yakınındaki işletmeler başarıyla getirildi."));
        }
        [AllowAnonymous]
        [HttpGet("storefronts/city")]
        public async Task<IActionResult>
            GetCityStorefronts(
                [FromQuery]
                string city,

                [FromQuery]
                double latitude,

                [FromQuery]
                double longitude,

                [FromQuery]
                int limit = 8)
        {
            if (
                string.IsNullOrWhiteSpace(
                    city) ||
                city.Trim().Length >
                    100
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .BadRequest,
                        "Şehir bilgisi geçersizdir."));
            }

            if (
                !double.IsFinite(
                    latitude) ||
                latitude < -90 ||
                latitude > 90
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .BadRequest,
                        "Enlem değeri -90 ile 90 arasında olmalıdır."));
            }

            if (
                !double.IsFinite(
                    longitude) ||
                longitude < -180 ||
                longitude > 180
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .BadRequest,
                        "Boylam değeri -180 ile 180 arasında olmalıdır."));
            }

            if (
                limit < 1 ||
                limit > 20
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .BadRequest,
                        "Limit değeri 1 ile 20 arasında olmalıdır."));
            }

            var result =
                await _producerService
                    .GetCityStorefrontsAsync(
                        city.Trim(),
                        latitude,
                        longitude,
                        limit);

            return Ok(
                ApiResponse<
                    List<
                        DiscoverProducerStorefrontResponse>>
                    .Succeed(
                        result,
                        "Şehrindeki işletmeler başarıyla getirildi."));
        }



        [AllowAnonymous]
        [HttpGet(
            "storefronts/{producerProfileId:int}/menu")]
        public async Task<IActionResult>
            GetAvailableStorefrontMenu(
                int producerProfileId)
        {
            if (producerProfileId <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "İşletme ID değeri sıfırdan büyük olmalıdır."));
            }

            var storefrontMenu =
                await _producerService
                    .GetAvailableStorefrontMenuAsync(
                        producerProfileId);

            if (storefrontMenu == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.NotFound,
                        "İşletme bulunamadı, şu anda hizmet vermiyor veya aktif menüsü bulunmuyor."));
            }

            return Ok(
                ApiResponse<
                    ProducerStorefrontMenuResponse>
                    .Succeed(
                        storefrontMenu,
                        "İşletme menüsü başarıyla getirildi."));
        }

        [Authorize(
            Policy =
                AuthorizationPolicies.ApprovedProducer)]
        [HttpGet("my-profile")]
        public async Task<IActionResult>
            GetMyProfile()
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
                    .GetMyApplicationAsync(
                        userId);

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
                ApiResponse<
                    ProducerApplicationStatusResponse>
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
        public async Task<IActionResult>
            UpdateMyProfile(
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

        [Authorize(
            Roles = UserRoles.Customer)]
        [HttpPost("apply")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult>
            Apply(
                [FromForm]
                ProducerApplicationRequest request)
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
                    await _producerService
                        .ApplyAsync(
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

        [Authorize(
            Roles = UserRoles.Customer)]
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
                    .GetMyApplicationAsync(
                        userId);

            if (application == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.NotFound,
                        "Bu kullanıcıya ait bir üretici başvurusu bulunamadı."));
            }

            return Ok(
                ApiResponse<
                    ProducerApplicationStatusResponse>
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