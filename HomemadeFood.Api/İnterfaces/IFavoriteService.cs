using HomemadeFood.Api.DTOs.Favorite;

namespace HomemadeFood.Api.Interfaces
{
    public interface IFavoriteService
    {
        Task<List<FavoriteResponse>> GetMyFavoritesAsync(
            int userId);

        Task<bool> AddFavoriteAsync(
            int userId,
            int foodId);

        Task<bool> RemoveFavoriteAsync(
            int userId,
            int foodId);
    }
}