using HomemadeFood.Api.DTOs.Cart;

namespace HomemadeFood.Api.Interfaces
{
    public interface ICartService
    {
        Task<CartResponse> GetCartAsync(
            int userId);

        Task<CartResponse?> AddItemAsync(
            int userId,
            AddCartItemRequest request);

        Task<CartResponse?> UpdateItemAsync(
            int userId,
            int cartItemId,
            UpdateCartItemRequest request);

        Task<CartResponse?> RemoveItemAsync(
            int userId,
            int cartItemId);

        Task<bool> ClearCartAsync(
            int userId);
    }
}