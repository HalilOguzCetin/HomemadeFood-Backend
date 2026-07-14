using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetByUserIdWithDetailsAsync(
            int userId);

        Task<Cart?> GetTrackedByUserIdWithItemsAsync(
            int userId);

        Task AddAsync(Cart cart);

        void Remove(Cart cart);

        Task SaveChangesAsync();
        void RemoveItem(CartItem cartItem);
        Task<Cart?> GetForOrderCreationAsync(int userId);
    }
}