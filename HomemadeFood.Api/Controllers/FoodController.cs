using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.DTOs.Food;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(
        Policy =
            AuthorizationPolicies
                .ApprovedProducer)]
    public class FoodController :
        ControllerBase
    {
        private readonly IFoodService
            _foodService;

        public FoodController(
            IFoodService foodService)
        {
            _foodService =
                foodService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult>
            GetAvailableFoods(
                [FromQuery]
                int? categoryId,

                [FromQuery]
                string? search)
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

            var foods =
                await _foodService
                    .GetAvailableFoodsAsync(
                        categoryId,
                        search);

            return Ok(
                ApiResponse<object>.Succeed(
                    foods,
                    "Yemekler başarıyla getirildi."));
        }

        /*
         * H5A:
         * Customer Home Popüler Yemekler carousel'i.
         */
        [AllowAnonymous]
        [HttpGet("popular")]
        public async Task<IActionResult>
            GetPopularFoods(
                [FromQuery]
                int limit = 8)
        {
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

            var foods =
                await _foodService
                    .GetPopularFoodsAsync(
                        limit);

            return Ok(
                ApiResponse<
                    List<PopularFoodResponse>>
                    .Succeed(
                        foods,
                        "Popüler yemekler başarıyla getirildi."));
        }
        [AllowAnonymous]
        [HttpGet("discover")]
        public async Task<IActionResult>
           DiscoverFoods(
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
                await _foodService
                    .GetDiscoverFoodsAsync(
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
                        DiscoverFoodResponse>>
                    .Succeed(
                        result,
                        "Yakınındaki yemekler başarıyla getirildi."));
        }


        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<IActionResult>
            GetFoodById(
                int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Yemek ID değeri sıfırdan büyük olmalıdır."));
            }

            var food =
                await _foodService
                    .GetAvailableFoodByIdAsync(
                        id);

            if (food == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .FoodNotFound,
                        "Yemek bulunamadı veya şu anda satışta değil."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    food,
                    "Yemek başarıyla getirildi."));
        }

        [HttpPost]
        [Consumes(
            "multipart/form-data")]
        [RequestSizeLimit(
            6 * 1024 * 1024)]
        public async Task<IActionResult>
            CreateFood(
                [FromForm]
                CreateFoodRequest request)
        {
            if (!TryGetUserId(
                    out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            if (
                request.Image == null ||
                request.Image.Length <= 0
            )
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Yemek fotoğrafı zorunludur."));
            }

            FoodResponse? food;

            try
            {
                food =
                    await _foodService
                        .CreateFoodAsync(
                            userId,
                            request);
            }
            catch (
                ArgumentException exception)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        exception.Message));
            }

            if (food == null)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .FoodCreationFailed,
                        "Yemek eklenemedi. Üretici profili veya kategori geçersiz olabilir."));
            }

            return StatusCode(
                StatusCodes
                    .Status201Created,

                ApiResponse<object>
                    .Succeed(
                        food,
                        "Yemek başarıyla oluşturuldu.",
                        ApiResponseCodes
                            .Created));
        }

        [HttpGet("my-foods")]
        public async Task<IActionResult>
            GetMyFoods()
        {
            if (!TryGetUserId(
                    out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var foods =
                await _foodService
                    .GetMyFoodsAsync(
                        userId);

            return Ok(
                ApiResponse<object>.Succeed(
                    foods,
                    "Üretici yemekleri başarıyla getirildi."));
        }

        [HttpGet(
            "my-foods/{id:int}")]
        public async Task<IActionResult>
            GetMyFoodById(
                int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Yemek ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetUserId(
                    out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var food =
                await _foodService
                    .GetMyFoodByIdAsync(
                        userId,
                        id);

            if (food == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .FoodNotFound,
                        "Yemek bulunamadı veya bu yemek size ait değil."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    food,
                    "Üretici yemeği başarıyla getirildi."));
        }

        [HttpPut("{id:int}")]
        [Consumes(
            "multipart/form-data")]
        [RequestSizeLimit(
            6 * 1024 * 1024)]
        public async Task<IActionResult>
            UpdateFood(
                int id,

                [FromForm]
                UpdateFoodRequest request)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Yemek ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetUserId(
                    out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            FoodResponse? food;

            try
            {
                food =
                    await _foodService
                        .UpdateFoodAsync(
                            userId,
                            id,
                            request);
            }
            catch (
                ArgumentException exception)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        exception.Message));
            }

            if (food == null)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .FoodUpdateFailed,
                        "Yemek güncellenemedi. Yemek bulunamamış, size ait olmayabilir veya kategori geçersiz olabilir."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    food,
                    "Yemek başarıyla güncellendi."));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult>
            DeleteFood(
                int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Yemek ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetUserId(
                    out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .Unauthorized,
                        "Kullanıcı bilgisi alınamadı."));
            }

            var result =
                await _foodService
                    .DeleteFoodAsync(
                        userId,
                        id);

            if (!result)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .FoodDeletionFailed,
                        "Yemek bulunamadı veya bu yemek size ait değil."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        foodId = id
                    },
                    "Yemek başarıyla satıştan kaldırıldı."));
        }

        private bool TryGetUserId(
            out int userId)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes
                        .NameIdentifier);

            return int.TryParse(
                userIdValue,
                out userId);
        }
    }
}