using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface IFavoriteRepository
    {
        Task AddAsync(Favorite favorite);

        Task<Favorite?> GetByUserIdAndFoodIdAsync(
            int userId,
            int foodId);

        Task<List<Favorite>> GetByUserIdAsync(
            int userId);

        void Remove(Favorite favorite);

        Task SaveChangesAsync();
    }
}