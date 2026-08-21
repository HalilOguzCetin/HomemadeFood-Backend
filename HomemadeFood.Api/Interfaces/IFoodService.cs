using HomemadeFood.Api.DTOs.Food;

namespace HomemadeFood.Api.Interfaces
{
    public interface IFoodService
    {
        Task<FoodResponse?> CreateFoodAsync(
            int userId,
            CreateFoodRequest request);

        Task<List<FoodResponse>>
            GetMyFoodsAsync(
                int userId);

        Task<FoodResponse?>
            GetMyFoodByIdAsync(
                int userId,
                int foodId);

        Task<FoodResponse?> UpdateFoodAsync(
            int userId,
            int foodId,
            UpdateFoodRequest request);

        Task<List<FoodResponse>>
            GetAvailableFoodsAsync(
                int? categoryId,
                string? search);
        Task<
           HomemadeFood.Api.DTOs.Common.PagedResultResponse<
               DiscoverFoodResponse>>
           GetDiscoverFoodsAsync(
               double latitude,
               double longitude,
               string city,
               double radiusKm,
               int page,
               int pageSize,
               int? categoryId,
               string? search);

        /*
         * H5A:
         * Gerçek kullanıcı davranışından hesaplanan
         * popüler yemek listesini döndürür.
         */
        Task<List<PopularFoodResponse>>
            GetPopularFoodsAsync(
                int limit);

        Task<bool> DeleteFoodAsync(
            int userId,
            int foodId);

        Task<FoodResponse?>
            GetAvailableFoodByIdAsync(
                int foodId);
    }
}