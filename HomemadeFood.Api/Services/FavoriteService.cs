using HomemadeFood.Api.DTOs.Favorite;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IFoodRepository _foodRepository;

        public FavoriteService(
            IFavoriteRepository favoriteRepository,
            IFoodRepository foodRepository)
        {
            _favoriteRepository = favoriteRepository;
            _foodRepository = foodRepository;
        }

        public async Task<List<FavoriteResponse>>
            GetMyFavoritesAsync(int userId)
        {
            var favorites =
                await _favoriteRepository
                    .GetByUserIdAsync(userId);

            return favorites
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<bool> AddFavoriteAsync(
            int userId,
            int foodId)
        {
            var food =
                await _foodRepository
                    .GetAvailableFoodByIdAsync(foodId);

            if (food == null)
            {
                return false;
            }

            var existingFavorite =
                await _favoriteRepository
                    .GetByUserIdAndFoodIdAsync(
                        userId,
                        foodId);

            if (existingFavorite != null)
            {
                return false;
            }

            var favorite = new Favorite
            {
                UserId = userId,
                FoodId = foodId,
                CreatedAt = DateTime.UtcNow
            };

            await _favoriteRepository.AddAsync(favorite);
            await _favoriteRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveFavoriteAsync(
            int userId,
            int foodId)
        {
            var favorite =
                await _favoriteRepository
                    .GetByUserIdAndFoodIdAsync(
                        userId,
                        foodId);

            if (favorite == null)
            {
                return false;
            }

            _favoriteRepository.Remove(favorite);

            await _favoriteRepository.SaveChangesAsync();

            return true;
        }

        private static FavoriteResponse MapToResponse(
            Favorite favorite)
        {
            return new FavoriteResponse
            {
                FavoriteId = favorite.Id,
                FoodId = favorite.FoodId,

                ProducerProfileId =
                    favorite.Food.ProducerProfileId,

                BusinessName =
                    favorite.Food.ProducerProfile.BusinessName,

                CategoryId =
                    favorite.Food.CategoryId,

                CategoryName =
                    favorite.Food.Category.Name,

                FoodName =
                    favorite.Food.Name,

                Description =
                    favorite.Food.Description,

                Price =
                    favorite.Food.Price,

                PreparationTimeMinutes =
                    favorite.Food.PreparationTimeMinutes,

                ImageUrl =
                    favorite.Food.ImageUrl,

                IsAvailable =
                    favorite.Food.IsAvailable,

                AddedAt =
                    favorite.CreatedAt
            };
        }
    }
}