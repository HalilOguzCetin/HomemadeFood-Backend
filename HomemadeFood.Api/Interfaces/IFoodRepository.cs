using HomemadeFood.Api.Entities;
using HomemadeFood.Api.ReadModels;

namespace HomemadeFood.Api.Interfaces
{
    public interface IFoodRepository
    {
        Task AddAsync(Food food);

        Task<List<Food>>
            GetByProducerProfileIdAsync(
                int producerProfileId);

        Task<Food?>
            GetByIdAndProducerProfileIdAsync(
                int foodId,
                int producerProfileId);

        Task<Category?>
            GetActiveCategoryByIdAsync(
                int categoryId);

        Task<List<Food>>
            GetAvailableFoodsAsync(
                int? categoryId,
                string? search);
        Task<List<FoodDiscoverCandidateReadModel>>
           GetDiscoverCandidatesAsync(
               int? categoryId,
               string? search,
               double minimumLatitude,
               double maximumLatitude,
               double minimumLongitude,
               double maximumLongitude,
               DateTime fromUtc);

        /*
         * H5A:
         * Popülerlik skoru için gerekli ham yemek +
         * sipariş + favori metriklerini döndürür.
         */
        Task<List<FoodPopularityCandidateReadModel>>
            GetPopularityCandidatesAsync(
                DateTime fromUtc);

        Task SaveChangesAsync();

        Task<Food?>
            GetAvailableFoodByIdAsync(
                int foodId);
    }
}