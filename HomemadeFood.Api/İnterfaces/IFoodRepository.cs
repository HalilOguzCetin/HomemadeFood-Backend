using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface IFoodRepository
    {
        Task AddAsync(Food food);

        Task<List<Food>> GetByProducerProfileIdAsync(
            int producerProfileId);

        Task<Food?> GetByIdAndProducerProfileIdAsync(
            int foodId,
            int producerProfileId);

        Task<Category?> GetActiveCategoryByIdAsync(
            int categoryId);

        

        Task SaveChangesAsync();
    }
}