using HomemadeFood.Api.DTOs.Food;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public class FoodService : IFoodService
    {
        private readonly IFoodRepository _foodRepository;
        private readonly IProducerRepository _producerRepository;
        private readonly IFoodImageStorageService _foodImageStorageService;

        public FoodService(
            IFoodRepository foodRepository,
            IProducerRepository producerRepository,
            IFoodImageStorageService foodImageStorageService)
        {
            _foodRepository = foodRepository;
            _producerRepository = producerRepository;
            _foodImageStorageService = foodImageStorageService;
        }

        public async Task<FoodResponse?> CreateFoodAsync(
            int userId,
            CreateFoodRequest request)
        {
            var producerProfile =
                await _producerRepository
                    .GetApprovedByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return null;
            }

            var category =
                await _foodRepository
                    .GetActiveCategoryByIdAsync(request.CategoryId);

            if (category == null)
            {
                return null;
            }

            if (request.Image == null)
            {
                throw new ArgumentException(
                    "Yemek fotoğrafı zorunludur.");
            }

            string? imageUrl = null;

            try
            {
                imageUrl =
                    await _foodImageStorageService
                        .SaveAsync(request.Image);

                var food = new Food
                {
                    ProducerProfileId = producerProfile.Id,
                    ProducerProfile = producerProfile,

                    CategoryId = category.Id,
                    Category = category,

                    Name = request.Name.Trim(),
                    Description = request.Description.Trim(),
                    Price = request.Price,
                    PreparationTimeMinutes =
                        request.PreparationTimeMinutes,

                    ImageUrl = imageUrl,

                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _foodRepository.AddAsync(food);
                await _foodRepository.SaveChangesAsync();

                return MapToResponse(food);
            }
            catch
            {
                /*
                 * Dosya yazıldıktan sonra DB kaydı başarısız olursa
                 * sunucuda sahipsiz görsel bırakmıyoruz.
                 */
                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    await SafeDeleteFoodImageAsync(
                        imageUrl);
                }

                throw;
            }
        }
        public async Task<List<FoodResponse>>
    GetAvailableFoodsAsync(
        int? categoryId,
        string? search)
        {
            var foods =
                await _foodRepository
                    .GetAvailableFoodsAsync(
                        categoryId,
                        search);

            return foods
                .Select(MapToResponse)
                .ToList();
        }
        public async Task<FoodResponse?>
    GetAvailableFoodByIdAsync(int foodId)
        {
            var food =
                await _foodRepository
                    .GetAvailableFoodByIdAsync(foodId);

            if (food == null)
            {
                return null;
            }

            return MapToResponse(food);
        }

        public async Task<List<FoodResponse>>
            GetMyFoodsAsync(int userId)
        {
            var producerProfile =
                await _producerRepository
                    .GetApprovedByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return new List<FoodResponse>();
            }

            var foods =
                await _foodRepository
                    .GetByProducerProfileIdAsync(
                        producerProfile.Id);

            return foods
                .Select(MapToResponse)
                .ToList();
        }
        public async Task<FoodResponse?> GetMyFoodByIdAsync(
    int userId,
    int foodId)
        {
            var producerProfile =
                await _producerRepository
                    .GetApprovedByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return null;
            }

            var food =
                await _foodRepository
                    .GetByIdAndProducerProfileIdAsync(
                        foodId,
                        producerProfile.Id);

            if (food == null)
            {
                return null;
            }

            return MapToResponse(food);
        }

        public async Task<FoodResponse?> UpdateFoodAsync(
            int userId,
            int foodId,
            UpdateFoodRequest request)
        {
            var producerProfile =
                await _producerRepository
                    .GetApprovedByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return null;
            }

            var food =
                await _foodRepository
                    .GetByIdAndProducerProfileIdAsync(
                        foodId,
                        producerProfile.Id);

            if (food == null)
            {
                return null;
            }

            var category =
                await _foodRepository
                    .GetActiveCategoryByIdAsync(
                        request.CategoryId);

            if (category == null)
            {
                return null;
            }

            var previousImageUrl =
                food.ImageUrl;

            string? replacementImageUrl =
                null;

            if (
                request.Image != null &&
                request.Image.Length > 0
            )
            {
                replacementImageUrl =
                    await _foodImageStorageService
                        .SaveAsync(request.Image);
            }

            food.CategoryId = category.Id;
            food.Category = category;

            food.Name = request.Name.Trim();
            food.Description = request.Description.Trim();
            food.Price = request.Price;

            food.PreparationTimeMinutes =
                request.PreparationTimeMinutes;

            /*
             * Yeni fotoğraf seçilmediyse mevcut görsel korunur.
             */
            food.ImageUrl =
                replacementImageUrl ??
                previousImageUrl;

            food.IsAvailable = request.IsAvailable;

            try
            {
                await _foodRepository
                    .SaveChangesAsync();
            }
            catch
            {
                /*
                 * DB update başarısız olduysa yeni yüklenen dosya
                 * sahipsiz bırakılmaz. Eski dosyaya dokunulmaz.
                 */
                await SafeDeleteFoodImageAsync(
                    replacementImageUrl);

                throw;
            }

            if (
                replacementImageUrl != null &&
                !string.Equals(
                    previousImageUrl,
                    replacementImageUrl,
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                /*
                 * DB artık yeni görsele işaret ediyor.
                 * Eski dosya temizliği başarısız olsa bile başarılı
                 * DB update'i geri çevirmiyoruz.
                 */
                await SafeDeleteFoodImageAsync(
                    previousImageUrl);
            }

            return MapToResponse(food);
        }

        public async Task<bool> DeleteFoodAsync(
            int userId,
            int foodId)
        {
            var producerProfile =
                await _producerRepository
                    .GetApprovedByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return false;
            }

            var food =
                await _foodRepository
                    .GetByIdAndProducerProfileIdAsync(
                        foodId,
                        producerProfile.Id);

            if (food == null)
            {
                return false;
            }

            food.IsAvailable = false;

            await _foodRepository.SaveChangesAsync();

            return true;
        }

        private async Task SafeDeleteFoodImageAsync(
            string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            try
            {
                await _foodImageStorageService
                    .DeleteAsync(imageUrl);
            }
            catch
            {
                /*
                 * Storage cleanup ikincil işlemdir.
                 * Başarılı DB kaydını veya asıl hatayı maskelemez.
                 */
            }
        }

        private static FoodResponse MapToResponse(Food food)
        {
            return new FoodResponse
            {
                Id = food.Id,
                ProducerProfileId =
                    food.ProducerProfileId,

                BusinessName =
                    food.ProducerProfile.BusinessName,

                CategoryId = food.CategoryId,
                CategoryName = food.Category.Name,

                Name = food.Name,
                Description = food.Description,
                Price = food.Price,

                PreparationTimeMinutes =
                    food.PreparationTimeMinutes,

                ImageUrl = food.ImageUrl,
                IsAvailable = food.IsAvailable,
                CreatedAt = food.CreatedAt
            };
        }
    }
}