using HomemadeFood.Api.DTOs.Food;

namespace HomemadeFood.Api.Interfaces
{
    public interface IFoodService
    {
        Task<FoodResponse?> CreateFoodAsync(
            int userId,
            CreateFoodRequest request);

        Task<List<FoodResponse>> GetMyFoodsAsync(
            int userId);

        Task<FoodResponse?> UpdateFoodAsync(
            int userId,
            int foodId,
            UpdateFoodRequest request);
        Task<List<FoodResponse>> GetAvailableFoodsAsync(
    int? categoryId,
    string? search);

        Task<bool> DeleteFoodAsync(
            int userId,
            int foodId);
        Task<FoodResponse?> GetAvailableFoodByIdAsync(int foodId);
    }
}