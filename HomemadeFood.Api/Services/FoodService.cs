using HomemadeFood.Api.DTOs.Food;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public class FoodService : IFoodService
    {
        private readonly IFoodRepository _foodRepository;
        private readonly IProducerRepository _producerRepository;

        public FoodService(
            IFoodRepository foodRepository,
            IProducerRepository producerRepository)
        {
            _foodRepository = foodRepository;
            _producerRepository = producerRepository;
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

                ImageUrl = request.ImageUrl?.Trim()
                    ?? string.Empty,

                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            };

            await _foodRepository.AddAsync(food);
            await _foodRepository.SaveChangesAsync();

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

            food.CategoryId = category.Id;
            food.Category = category;

            food.Name = request.Name.Trim();
            food.Description = request.Description.Trim();
            food.Price = request.Price;

            food.PreparationTimeMinutes =
                request.PreparationTimeMinutes;

            food.ImageUrl =
                request.ImageUrl?.Trim() ?? string.Empty;

            food.IsAvailable = request.IsAvailable;

            await _foodRepository.SaveChangesAsync();

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